using HebronPay.Authentication;
using HebronPay.Model;
using HebronPay.Responses;
using HebronPay.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace HebronPay.Services.Implementation
{
    public class AuthenticationServices : IAuthenticationServices
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> roleManager;


        private ApplicationDbContext _context;
        public AuthenticationServices(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, ApplicationDbContext context)
        {
            this.userManager = userManager;
            _configuration = configuration;
            this.roleManager = roleManager;
            _context = context;
        }

        public async Task<ApiResponse> CreateOTP(string email)
        {
            ReturnedResponse returnedResponse = new ReturnedResponse();
            try
            {
                Random random = new Random();
                int pin = random.Next(10000, 99999);

                var newOTP = new OTP
                {
                    pin = pin,
                    email = email,

                };
                await _context.OTPs.AddAsync(newOTP);
                await _context.SaveChangesAsync();

                return returnedResponse.CorrectResponse(newOTP.pin);
            }

            catch (Exception myEx)
            {
                return returnedResponse.ErrorResponse(myEx.ToString(), null);
            }




        }

        public async Task<string> CreateRoles()
        {
            //create the "User" Role
            if (!await roleManager.RoleExistsAsync(UserRoles.User))
            {
                await roleManager.CreateAsync(new IdentityRole(UserRoles.User));
                return "Role successfully created";
            }
            return "Role already exists";
        }

        public async Task<ApiResponse> SendOTP( string email)
        {
            ReturnedResponse returnedResponse = new ReturnedResponse();
            var newOTP = await CreateOTP(email);

            if (newOTP.error != null)
            {
                return returnedResponse.ErrorResponse(newOTP.error.message, null);
            }

            HttpClient client = new HttpClient();
            string baseUrl = "https://rapidprod-sendgrid-v1.p.rapidapi.com/";

            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-RapidAPI-Key", "12aaead381mshb7c62fe6ca523a7p122464jsn284af6ccfc7b");
            client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "rapidprod-sendgrid-v1.p.rapidapi.com");

            SendEmailRequest request = new SendEmailRequest
            {
                personalizations = new List<Personalization>()
                {
                    new Personalization
                    {
                        to = new List<To>()
                        {
                            new To
                            {
                                email = email,
                            }
                        },
                        subject = "YOUR ONE TIME PASSOWRD (OTP)",
                    }
                },

                from = new From
                {
                    email = "chizaramonuorah50@gmail.com",
                },

                content = new List<Content>()
                {
                    new Content
                    {
                        type = "text/plain",
                         value = $"YOUR ONE TIME PASSWORD TO LOG IN TO HEBRONPAY HAS BEEN GENERATED. DO NOT GIVE ANYONE, IT IS {newOTP.data}",
                    }
                }

            };

            try
            {
                string path = "mail/send";
                HttpResponseMessage Res = await client.PostAsJsonAsync(path, request);

                if (Res.IsSuccessStatusCode)
                {
                    var response = await Res.Content.ReadAsStringAsync();
                    return returnedResponse.CorrectResponse("Email Successfully Sent");

                }

                return returnedResponse.ErrorResponse(Res.Content.ToString(), null);

            }

            catch (Exception my_ex)
            {
                return returnedResponse.ErrorResponse(my_ex.Message.ToString(), null);

            }


        }

        public async Task<ApiResponse> SignUpUser(SignUpModel model)
        {
            ReturnedResponse returnedResponse = new ReturnedResponse();

            await CreateRoles();

            if (String.IsNullOrEmpty(model.UserName))
            {
                return returnedResponse.ErrorResponse("Username cannot be empty", null);

            }

            if (String.IsNullOrEmpty(model.Email))
            {
                return returnedResponse.ErrorResponse("Email cannot be empty", null);
            }


            //ensure no other user has the same username
            var userExists = await userManager.FindByNameAsync(model.UserName);
            if (userExists != null)
            {
                return returnedResponse.ErrorResponse("User with that Username Already Exists", null);
            }

            //ensure no other user has the same email
            var emailExists = await userManager.FindByEmailAsync(model.Email);
            if (emailExists != null)
            {
                return returnedResponse.ErrorResponse("User with that Email Already Exists", null);
            }

            //ensure no other user has the same phone number
            var phoneExists = _context.Users.Where(u => u.PhoneNumber == model.PhoneNumber);
            if (phoneExists != null)
            {
                return returnedResponse.ErrorResponse("User with that Phone number Already Exists", null);
            }


            //create the application user instance

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Gender = model.Gender,
                DateofBirth = model.DateofBirth,
                PhoneNumber = model.PhoneNumber,

                

            };

            //use the user manager to create the user in the database
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return returnedResponse.ErrorResponse("User Not Created", null);
            }

            //create a user role for the user and save changes to the database
            await userManager.AddToRoleAsync(user, UserRoles.User);
            await _context.SaveChangesAsync();
            return returnedResponse.CorrectResponse(user);



        }

        public async Task<ApiResponse> ValidateOTP(int inputPin, string email)
        {
            ReturnedResponse returnedResponse = new ReturnedResponse();
            var userOTP = await _context.OTPs.Where(o => o.email == email).OrderBy(o => o.Id).LastAsync();
            if (userOTP.pin == inputPin)
            {
                return returnedResponse.CorrectResponse("OTP successfully validated");
            }
            return returnedResponse.ErrorResponse("Invalid OTP", null);
        }
    }
}
