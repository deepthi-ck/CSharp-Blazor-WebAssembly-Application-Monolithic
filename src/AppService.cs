using System;
using System.Collections.Generic;

namespace BlazorWasmMonolith
{
    public sealed class AppService
    {
        private readonly AppManager _manager;

        public AppService(AppManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }

            _manager = manager;
        }

        public AppManager Manager { get { return _manager; } }

        public AppResponse Put(AppRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            return _manager.Put(request.Key, request.Value, request.Ttl);
        }

        public AppResponse Get(string key)
        {
            return _manager.Get(key);
        }

        public AppResponse Delete(string key)
        {
            return _manager.Delete(key);
        }

        public IList<object> ListResources()
        {
            return _manager.ListResources();
        }

        public IList<object> ListNodes()
        {
            return _manager.ListNodes();
        }
    }
}
