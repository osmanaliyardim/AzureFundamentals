using AzureADB2CWeb.Data;
using AzureADB2CWeb.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

string Tenant = "AzureADB2COsman.onmicrosoft.com";
string AzureADB2CHostName = "azureadb2cosman.b2clogin.com";
string ClientId = "d7f3dbe3-3ea7-4da3-ba4d-0b9022d279c6";
string PolicySignUpSignIn = "B2C_1_SignIn_Up";
string PolicyEditProfile = "B2C_1_Edit";
string Scope = "https://AzureADB2COsman.onmicrosoft.com/azureB2CAPI/fullAccess";
string ClientSecret = "U5s8Q~Gd~piIPPTOKU5wG.5RQZvLLBw~E-ynFaX6";

string AuthorityBase = $"https://{AzureADB2CHostName}/{Tenant}/";
string AuthoritySignInUp = $"{AuthorityBase}{PolicySignUpSignIn}/v2.0";
string AuthorityEditProfile = $"{AuthorityBase}{PolicyEditProfile}/v2.0";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Entra Client ID: c128342f-8055-4683-8fc1-73ba048f2a91
//Auth Endpoint: https://login.microsoftonline.com/5e7f43d7-0878-463f-8a25-e1b6488f71c9/oauth2/v2.0/authorize
builder.Services.AddHttpClient();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
}).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
      .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
      {
          options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
          options.Authority = AuthoritySignInUp;
          options.ClientId = ClientId;
          options.ResponseType = "code";
          options.SaveTokens = true;
          options.Scope.Add(Scope);
          options.ClientSecret = ClientSecret;
          options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = "name" };
          //options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" };

          //// if we want to use as Authorization(Roles = "contractor") in controller
          //options.Events = new OpenIdConnectEvents
          //{
          //    OnTokenValidated = async opt =>
          //    {
          //        string role = opt.Principal.FindFirstValue("extension_UserRole");

          //        var claims = new List<Claim>
          //        {
          //            new Claim(ClaimTypes.Role, role)
          //        };
          //        var appIdentity = new ClaimsIdentity(claims);
          //        opt.Principal.AddIdentity(appIdentity);
          //    }
          //};
      })
      .AddOpenIdConnect("B2C_1_Edit", GetOpenIdConnectOptions("B2C_1_Edit"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


Action<OpenIdConnectOptions> GetOpenIdConnectOptions(string policy) => options =>
{
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.Authority = AuthorityEditProfile;
    options.ClientId = ClientId;
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.Scope.Add(Scope);
    options.ClientSecret = ClientSecret;
    options.CallbackPath = "/signin-oidc-" + policy;
    options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = "name" };
    options.Events = new OpenIdConnectEvents
    {
        OnMessageReceived = context =>
        {
            if (!string.IsNullOrEmpty(context.ProtocolMessage.Error) &&
                !string.IsNullOrEmpty(context.ProtocolMessage.ErrorDescription))
            {
                if (context.ProtocolMessage.Error.Contains("access_denied"))
                {
                    context.HandleResponse();
                    context.Response.Redirect("/");
                }
            }

            return Task.FromResult(0);
        }
    };
    
    // this is for 3rd party identity providers like Facebook, Google, MS
    //options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" };
};