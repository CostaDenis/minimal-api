namespace minimal_api.Domain.ModelViews
{
    public struct Home
    {
        public string Doc { get => "/swagger"; }
        public string Message { get => "Bem vindo ao Minimal API com ASP.NET Core e MySQL!"; }
    }
}