using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repository;
using Service; // Chứa các Service cũ (ExamExport, ExtractZip...)
using Service.Interfaces; // Chứa IUserService
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ====================================================
// 1. CẤU HÌNH CONTROLLER & SWAGGER
// ====================================================
builder.Services.AddControllers();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// Cấu hình Swagger + Nút Authorize
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Exam Checker API", Version = "v1" });

    // Cấu hình Security Scheme (Http Bearer)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Nhập token vào ô bên dưới (Không cần chữ 'Bearer ' ở đầu)"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ====================================================
// 2. CẤU HÌNH JWT AUTHENTICATION
// ====================================================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration["JwtSettings:SecretKey"]!)),
            RoleClaimType = ClaimTypes.Role
        };
    });

// ====================================================
// 3. ĐĂNG KÝ DEPENDENCY INJECTION (GỘP CẢ 2 BÊN)
// ====================================================

// A. UnitOfWork & Repository
builder.Services.AddScoped<UnitOfWork>();
builder.Services.AddScoped(typeof(GenericRepository<>)); // Giữ lại để đảm bảo tính năng cũ không lỗi
builder.Services.AddScoped<IScanRepository, ScanRepository>();
// B. User Services (Phần của bạn)
builder.Services.AddScoped<IUserService, UserService>();

// C. Student Services
builder.Services.AddScoped<IStudentService_, StudentService>();

// D. Exam & Submission Services (Phần của nhóm/Upstream)
builder.Services.AddScoped<IExamExportService, ExamExportService>();
builder.Services.AddScoped<IExtractZipService, ExtractZipService>();
builder.Services.AddScoped<ISubmissionService, SubmissionService>();
builder.Services.AddScoped<IScanHardCodeService, ScanHardCodeService>();
builder.Services.AddScoped<IZipExtractService, ZipExtractService>();
// ====================================================
// 4. BUILD APP & MIDDLEWARE
// ====================================================
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Thứ tự quan trọng: Authen -> Author
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// (Tùy chọn) Seed Data nếu cần
// await DbInitializer.SeedAdminUser(app);

app.Run();