namespace ASPWebSolution
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // WebApplication ������: �⺻ ���� �� ���� �����̳� ���� ����
            var builder = WebApplication.CreateBuilder(args);

            // ----------------------------
            // HttpClient ���� ���
            // ----------------------------
            // �̸��� "MyWebClient"�� HttpClient�� ���
            // BaseAddress�� �ܺ� API(Python Uvicorn ����)�� �ּҷ� ����
            builder.Services.AddHttpClient("MyWebClient", client =>
            {
                client.BaseAddress = new Uri("http://localhost:8000");  // Python Uvicorn ���� �ּ�
            });

            // ----------------------------
            // CORS (Cross-Origin Resource Sharing) ��å ����
            // ----------------------------
            // ���� ȯ�濡���� ��� ��û ��� (���Ȼ� � ȯ�濡���� �����ؾ� ��)
            builder.Services.AddCors(options => {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()   // � �����ο��� ��û�� �͵� ���
                          .AllowAnyMethod()   // GET, POST, PUT, DELETE �� ��� HTTP �޼��� ���
                          .AllowAnyHeader();  // ��� ��� ���
                });
            });

            // ----------------------------
            // MVC ��Ʈ�ѷ� ���
            // ----------------------------
            // [ApiController] �Ӽ��� ����ϴ� ��Ʈ�ѷ� ��� API�� ����� �� �ְ� ��
            builder.Services.AddControllers();

            // ----------------------------
            // ���ø����̼� ����
            // ----------------------------
            var app = builder.Build();

            // ----------------------------
            // �̵���� ���������� ����
            // ----------------------------

            app.UseCors();              // ������ ������ CORS ��å�� ������ ���
            app.UseDefaultFiles();      // �⺻ ����(index.html ��)�� ��Ʈ ��û���� �����ϵ��� ����
            app.UseStaticFiles();       // wwwroot ������ ���� ����(css, js, �̹��� ��)�� ����

            app.MapControllers();       // ��Ʈ�ѷ����� ������ ������� ���

            // ----------------------------
            // ���ø����̼� ����
            // ----------------------------
            app.Run();                  // �� ���� ����, ��û ���� ���
        }
    }
}
