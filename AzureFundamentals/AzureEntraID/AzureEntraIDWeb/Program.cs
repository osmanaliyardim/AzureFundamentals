using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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
          options.Authority = "https://login.microsoftonline.com/5e7f43d7-0878-463f-8a25-e1b6488f71c9/v2.0";
          options.ClientId = "c128342f-8055-4683-8fc1-73ba048f2a91";
          options.ResponseType = "code";
          options.SaveTokens = true;
          options.Scope.Add("api://54f5abff-62ab-4911-92d6-c9d8f9a11bee/AdminAccess");
          options.ClientSecret = "tK98Q~gqhwK1L7jE4WS-E3qPBI_L9~eYcWggtb0T";
          options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = "name" };
      });

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
