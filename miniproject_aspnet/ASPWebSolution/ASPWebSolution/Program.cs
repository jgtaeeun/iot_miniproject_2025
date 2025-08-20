namespace ASPWebSolution
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // WebApplication 생성자: 기본 설정 및 서비스 컨테이너 구성 시작
            var builder = WebApplication.CreateBuilder(args);

            // ----------------------------
            // HttpClient 서비스 등록
            // ----------------------------
            // 이름이 "MyWebClient"인 HttpClient를 등록
            // BaseAddress는 외부 API(Python Uvicorn 서버)의 주소로 설정
            builder.Services.AddHttpClient("MyWebClient", client =>
            {
                client.BaseAddress = new Uri("http://localhost:8000");  // Python Uvicorn 서비스 주소
            });

            // ----------------------------
            // CORS (Cross-Origin Resource Sharing) 정책 설정
            // ----------------------------
            // 개발 환경에서는 모든 요청 허용 (보안상 운영 환경에서는 제한해야 함)
            builder.Services.AddCors(options => {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()   // 어떤 도메인에서 요청이 와도 허용
                          .AllowAnyMethod()   // GET, POST, PUT, DELETE 등 모든 HTTP 메서드 허용
                          .AllowAnyHeader();  // 모든 헤더 허용
                });
            });

            // ----------------------------
            // MVC 컨트롤러 등록
            // ----------------------------
            // [ApiController] 속성을 사용하는 컨트롤러 기반 API를 사용할 수 있게 함
            builder.Services.AddControllers();

            // ----------------------------
            // 애플리케이션 빌드
            // ----------------------------
            var app = builder.Build();

            // ----------------------------
            // 미들웨어 파이프라인 구성
            // ----------------------------

            app.UseCors();              // 위에서 정의한 CORS 정책을 실제로 사용
            app.UseDefaultFiles();      // 기본 파일(index.html 등)을 루트 요청으로 응답하도록 설정
            app.UseStaticFiles();       // wwwroot 폴더의 정적 파일(css, js, 이미지 등)을 서빙

            app.MapControllers();       // 컨트롤러에서 정의한 라우팅을 사용

            // ----------------------------
            // 애플리케이션 실행
            // ----------------------------
            app.Run();                  // 웹 서버 실행, 요청 수신 대기
        }
    }
}
