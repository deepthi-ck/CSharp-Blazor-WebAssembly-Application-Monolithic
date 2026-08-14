(function (global) {
  var PAGES = [
    { id: "dashboard", href: "/", label: "Dashboard" },
    { id: "resources", href: "/resources.html", label: "Resources" },
    { id: "stats", href: "/stats.html", label: "Statistics" },
    { id: "nodes", href: "/nodes.html", label: "Partition nodes" },
    { id: "health", href: "/health.html", label: "Health" },
    { id: "version", href: "/version.html", label: "Version" }
  ];

  function $(id) { return document.getElementById(id); }

  function renderSidebar(active) {
    var aside = $("sidebar");
    if (!aside) { return; }
    var nav = "";
    for (var i = 0; i < PAGES.length; i++) {
      var page = PAGES[i];
      var cls = page.id === active ? "active" : "";
      nav += '<a class="' + cls + '" href="' + page.href + '">' + page.label + "</a>";
    }
    aside.innerHTML =
      '<div class="brand">' +
      '<p class="kicker">deepthi-ck</p>' +
      "<h1>Blazor WebAssembly</h1>" +
      "</div>" +
      '<nav class="nav" aria-label="Primary">' + nav + "</nav>";
  }

  function api(path, options) {
    return fetch(path, options || {}).then(function (res) {
      return res.text().then(function (text) {
        var body = text;
        try { body = text ? JSON.parse(text) : {}; } catch (e) { body = { raw: text }; }
        return { ok: res.ok, status: res.status, body: body };
      });
    });
  }

  function fillMeta(version) {
    var branch = $("meta-branch");
    var customer = $("meta-customer");
    var tfm = $("meta-tfm");
    if (branch) { branch.textContent = "Branch " + (version.branch || "unknown"); }
    if (customer) { customer.textContent = "Customer " + (version.customer_version || "unknown"); }
    if (tfm) { tfm.textContent = version.target_framework || ""; }
  }

  function setText(id, value) {
    var el = $(id);
    if (el) { el.textContent = value; }
  }

  function boot(active) {
    renderSidebar(active);
    api("/version").then(function (res) {
      if (res.ok) { fillMeta(res.body); }
    });
    if (active === "dashboard") { loadDashboard(); }
    if (active === "resources") { loadResources(); }
    if (active === "stats") { loadStats(); }
    if (active === "nodes") { loadNodes(); }
    if (active === "health") { loadHealth(); }
    if (active === "version") { loadVersion(); }
  }

  function loadDashboard() {
    Promise.all([api("/health"), api("/version"), api("/app/stats"), api("/app/resources")]).then(function (parts) {
      var health = parts[0].body || {};
      var version = parts[1].body || {};
      var stats = parts[2].body || {};
      var resources = parts[3].body || [];
      setText("kpi-health", health.status || "unknown");
      setText("kpi-nodes", String(health.nodes == null ? 0 : health.nodes));
      setText("kpi-entries", String(stats.entries == null ? 0 : stats.entries));
      setText("kpi-branch", version.branch || "unknown");
      setText("dash-scenario", version.scenario || "");
      setText("dash-app", version.application || "");
      var body = $("dash-inventory");
      if (!body) { return; }
      if (!resources.length) {
        body.innerHTML = "<tr><td colspan='3'>No catalog rows yet. Open Resources to PUT product:1001.</td></tr>";
        return;
      }
      var html = "";
      for (var i = 0; i < resources.length; i++) {
        html += "<tr><td>" + resources[i].key + "</td><td>" + resources[i].value + "</td><td>" + resources[i].node + "</td></tr>";
      }
      body.innerHTML = html;
    });
  }

  function loadResources() {
    var keyEl = $("resource-key");
    var valueEl = $("resource-value");
    var out = $("resource-output");
    function refresh() {
      api("/app/resources").then(function (res) {
        var body = $("resource-rows");
        var rows = res.body || [];
        if (!body) { return; }
        if (!rows.length) {
          body.innerHTML = "<tr><td colspan='3'>Store is empty. PUT a catalog key such as product:1001.</td></tr>";
          return;
        }
        var html = "";
        for (var i = 0; i < rows.length; i++) {
          html += "<tr><td>" + rows[i].key + "</td><td>" + rows[i].value + "</td><td>" + rows[i].node + "</td></tr>";
        }
        body.innerHTML = html;
      });
    }
    function write(label, payload) {
      if (out) { out.textContent = label + "\n" + JSON.stringify(payload, null, 2); }
      refresh();
    }
    $("btn-get").onclick = function () {
      api("/app/" + encodeURIComponent(keyEl.value)).then(function (res) { write("GET", res.body); });
    };
    $("btn-put").onclick = function () {
      api("/app/" + encodeURIComponent(keyEl.value), {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ value: valueEl.value })
      }).then(function (res) { write("PUT", res.body); });
    };
    $("btn-delete").onclick = function () {
      api("/app/" + encodeURIComponent(keyEl.value), { method: "DELETE" }).then(function (res) { write("DELETE", res.body); });
    };
    refresh();
  }

  function loadStats() {
    api("/app/stats").then(function (res) {
      var s = res.body || {};
      var ids = ["hits", "misses", "puts", "gets", "deletes", "replications", "evictions", "expirations", "entries", "nodes"];
      for (var i = 0; i < ids.length; i++) {
        setText("stat-" + ids[i], String(s[ids[i]] == null ? 0 : s[ids[i]]));
      }
    });
  }

  function loadNodes() {
    api("/app/nodes").then(function (res) {
      var wrap = $("node-cards");
      if (!wrap) { return; }
      var nodes = res.body || [];
      var html = "";
      for (var i = 0; i < nodes.length; i++) {
        var n = nodes[i];
        html += '<article class="card"><p class="kpi-label">' + n.id + '</p><p class="kpi">' + n.entries + '</p><p class="hint">Keys: ' + ((n.keys || []).join(", ") || "none") + "</p></article>";
      }
      wrap.innerHTML = html;
    });
  }

  function loadHealth() {
    api("/health").then(function (res) {
      var h = res.body || {};
      setText("health-status", h.status || "unknown");
      setText("health-runtime", h.blazor_wasm_app || "unknown");
      setText("health-nodes", String(h.nodes == null ? 0 : h.nodes));
      setText("health-scenario", h.scenario || "");
      setText("health-builtins", h.csharp_builtins || "");
      setText("health-json", JSON.stringify(h, null, 2));
    });
  }

  function loadVersion() {
    api("/version").then(function (res) {
      var v = res.body || {};
      setText("ver-application", v.application || "");
      setText("ver-scenario", v.scenario || "");
      setText("ver-module", v.module || "");
      setText("ver-customer", v.customer_version || "");
      setText("ver-branch", v.branch || "");
      setText("ver-tfm", v.target_framework || "");
      setText("ver-app", v.app || "");
      setText("ver-json", JSON.stringify(v, null, 2));
    });
  }

  global.BlazorUi = { boot: boot };
})(window);
