namespace BlazorWasmMonolith.Tests
{
    public static class AppServiceTest
    {
        public static int Run()
        {
            AppService service = new AppService(new AppManager(AppConfiguration.CreateDefault()));
            AppResponse put = service.Put(new AppRequest("PUT", "customer:3001", "Ada", null));
            Expect.Equal("SUCCESS", put.Status);
            Expect.Equal("Ada", service.Get("customer:3001").Value);
            Expect.Equal("SUCCESS", service.Delete("customer:3001").Status);
            Expect.Equal("NOT_FOUND", service.Get("customer:3001").Status);
            return 0;
        }
    }
}
