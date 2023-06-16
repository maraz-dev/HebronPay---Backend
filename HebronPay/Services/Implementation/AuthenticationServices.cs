using AutoMapper;
using HebronPay.Authentication;
using HebronPay.FlutterwaveServices.Interface;
using HebronPay.Model;
using HebronPay.Model.FlutterWave;
using HebronPay.Model.FlutterWave.SubAccout;
using HebronPay.Model.RapidAPI;
using HebronPay.Responses;
using HebronPay.Responses.Enums;
using HebronPay.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HebronPay.Services.Implementation
{
    public class AuthenticationServices : IAuthenticationServices
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration _configuration;
        private readonly IFlutterwaveServices _flutterwaveServices;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly RapidAPISettings _rapidAPISettings;


        private ApplicationDbContext _context;
        public AuthenticationServices(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<RapidAPISettings> rapidApiSettings, IConfiguration configuration, ApplicationDbContext context, IFlutterwaveServices flutterwaveServices)
        {
            this.userManager = userManager;
            _configuration = configuration;
            this.roleManager = roleManager;
            _context = context;
            _flutterwaveServices = flutterwaveServices;
            _rapidAPISettings = rapidApiSettings.Value;
        }


        public async Task<ApiResponse> CheckValidations(ValidateModel model)
        {
            ReturnedResponse returnedResponse = new ReturnedResponse();

            //create the "user" role
            await CreateRoles();

            if (String.IsNullOrEmpty(model.UserName))
            {
                return returnedResponse.ErrorResponse("Username cannot be empty", null);

            }

            if (String.IsNullOrEmpty(model.Email))
            {
                return returnedResponse.ErrorResponse("Email cannot be empty", null);

            }

            //ensure that the email is valid using regular expressions.
            var validateEmail = ValidateEmailRegExp(model.Email);
            if (!validateEmail)
            {
                return returnedResponse.ErrorResponse("Email is Invalid", null);
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

            //ensure password meets the validations
            var validatePassword = ValidatePassword(model.Password);
            if (validatePassword.error != null)
            {
                return returnedResponse.ErrorResponse(validatePassword.error.message, null);
            }

            //ensure password and confirm password are the same
            if (model.Password != model.ConfirmPassword)
            {
                return returnedResponse.ErrorResponse("Password and Confirm Password do not match", null);
            }


            return returnedResponse.CorrectResponse("Valid Details");

        }


        public bool ValidateEmailRegExp(string email)
        {
            var validEmail = new Regex("^\\S+@\\S+\\.\\S+$");
            if (validEmail.IsMatch(email))
            {
                return true;
            }
            return false;

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
                    dateExpired = DateTime.Now.AddMinutes(1),

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

            client.DefaultRequestHeaders.Add("X-RapidAPI-Key", _rapidAPISettings.rapidApiKey);
            client.DefaultRequestHeaders.Add("X-RapidAPI-Host", _rapidAPISettings.rapidApiHost);



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


            var mapper = new Mapper(MapperConfig.GetMapperConfiguration());
            var validateModel = mapper.Map<ValidateModel>(model);


            //just for safety, validate everything again just to be sure
            var validations = await CheckValidations(validateModel);

            if (validations.Message != ApiResponseEnum.success.ToString())
            {
                return returnedResponse.ErrorResponse(validations.error.message, null);
            }


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
            var phoneExists = await _context.Users.Where(u => u.PhoneNumber == model.PhoneNumber).AnyAsync();
            if (phoneExists)
            {
                return returnedResponse.ErrorResponse("User with that Phone number Already Exists", null);
            }

            //try to create the wallet subaccount with flutterwave
            var flutterwaveResponse = await _flutterwaveServices.createSubAccount(new CreateSubAccountRequestModel
            {
                account_name = $"{model.FirstName} {model.LastName}",
                account_reference = generateRandomString(20),
                bank_code = "232",
                country = "NG",
                email = model.Email,
                mobilenumber = model.PhoneNumber
            });
            
            
            if(flutterwaveResponse.status != FlutterWaveResponseEnum.success.GetEnumDescription())
            {
                return returnedResponse.ErrorResponse(flutterwaveResponse.message, null);
            }

            CreateSubAccountResponseModel newSubAccount = JsonConvert.DeserializeObject<CreateSubAccountResponseModel>(flutterwaveResponse.data.ToString());

            
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
                subAccountId = newSubAccount.id,
                
                //subAccount = mapper.Map<SubAccount>(newSubAccount),
                
                subAccount = new SubAccount
                {
                    account_name = $"{newSubAccount.account_name} HEBRONPAY",
                    account_reference = newSubAccount.account_reference,
                    bank_code = newSubAccount.bank_code,
                    country = newSubAccount.country,
                    flutterwaveSubAccountId = newSubAccount.id,
                    bank_name = newSubAccount.bank_name,
                    barter_id = newSubAccount.barter_id,
                    created_at = newSubAccount.created_at,
                    email = newSubAccount.email,
                    mobilenumber = newSubAccount.mobilenumber,
                    nuban = newSubAccount.nuban,
                    status = newSubAccount.status

                },
                
                isOtpVerified = false,
                isPinSet = false,
                
                hebronPayWallet = new HebronPayWallet
                {
                    walletBalance = 0.0,
                    //hebronPayTransactions = new List<HebronPayTransaction>() { },
                    
                },


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

        public async Task<ApiResponse> ValidateOTP(string inputPin, string email)
        {
            ReturnedResponse returnedResponse = new ReturnedResponse();
            var userOTP = await _context.OTPs.Where(o => o.email == email).OrderBy(o => o.Id).LastAsync();

            

            if (userOTP.pin == Convert.ToInt32(inputPin))
            {
                if (DateTime.Now > userOTP.dateExpired)
                {
                    return returnedResponse.ErrorResponse("This otp has expired", null);
                }

                var user = await userManager.FindByEmailAsync(email);
                if(user == null)
                {
                    return returnedResponse.ErrorResponse("User does not exist", null);

                }
                user.isOtpVerified = true;

                await _context.SaveChangesAsync();
                
                return returnedResponse.CorrectResponse("OTP successfully validated");
            }
            return returnedResponse.ErrorResponse("Invalid OTP", null);
        }


        public string generateRandomString(int length)
        {

            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }

            return new string(result);
        }

        public async Task<ApiResponse> Login(LoginModel model)
        {
            ReturnedResponse returnedResponse = new ReturnedResponse();

            if (String.IsNullOrEmpty(model.Email))
            {
                return returnedResponse.ErrorResponse("Username cannot be empty", null);

            }


            //check if there is any user with that email or username
            var user = await userManager.FindByNameAsync(model.Email) ?? await userManager.FindByEmailAsync(model.Email);

            //if user does not exist, return an error response
            if (user == null) return returnedResponse.ErrorResponse("No User exists with that Username or Email", null);


            //check if the password used is correct
            bool correctPassword = await userManager.CheckPasswordAsync(user, model.Password);

            //if password is incorrect, return an error response
            if (!correctPassword) return returnedResponse.ErrorResponse("Incorrect Login Details", null);


            //collect the subacount and wallet details of the user
            user = await _context.Users.Where(u => u.Email == model.Email || u.UserName == model.Email)
                .Include(u => u.subAccount)
                .Include(u=>u.hebronPayWallet)
                //.ThenInclude(w=>w.hebronPayTransactions)
                .FirstAsync();

            //if the user is not null and the password is correct, assign roles and claims to the user
            try
            {
                var userRoles = await userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,user.UserName),
                    new Claim(ClaimTypes.Email,user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }


                //generate the JSON Web Token for the User
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );


                //create the Token Object for the JSON Web Token
                AuthorizationToken authorizationToken = new AuthorizationToken
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration = token.ValidTo,
                    TokenUser = user.UserName
                };


                //use automapper to return all the user's information, along with their generated JWT
                var mapper = new Mapper(MapperConfig.GetMapperConfiguration());


                var loginResponseModel = mapper.Map<LoginResponseModel>(user);
                loginResponseModel.Token = new JwtSecurityTokenHandler().WriteToken(token);
                loginResponseModel.Expiration = token.ValidTo;
                loginResponseModel.TokenUser = user.UserName;
                loginResponseModel.subAccount = user.subAccount;
                loginResponseModel.hebronPayWallet = user.hebronPayWallet;


                return returnedResponse.CorrectResponse(loginResponseModel);

            }

            catch (Exception myEx)
            {
                return returnedResponse.ErrorResponse(myEx.Message, null);
            }




        }

        public async Task<ApiResponse> SetPin(string username, SetPinModel model)
        {
            ReturnedResponse returnedResponse = new ReturnedResponse();

            if (model.walletPin != model.confirmWalletPin)
            {
                return returnedResponse.ErrorResponse("Pin and Confirm pin do not match", null);
            }

            //ensure pin meets the validations
            var validatePin = ValidatePin(model.walletPin.ToString());
            if (validatePin.error != null)
            {
                return returnedResponse.ErrorResponse(validatePin.error.message, null);
            }

            var user = _context.Users.Where(u=>u.UserName == username)
                .Include(u=>u.hebronPayWallet)
                .Include(u=>u.subAccount)
                .FirstOrDefault();

            user.hebronPayWallet.walletPin = model.walletPin;
            user.isPinSet = true;
            await _context.SaveChangesAsync();
            return returnedResponse.CorrectResponse("successfully set pin");
        }

        public async Task<ApiResponse> ChangePin(string username, ChangePinModel model)
        {
            //throw new NotImplementedException();
            ReturnedResponse returnedResponse = new ReturnedResponse();


            

            var user = _context.Users.Where(u => u.UserName == username)
                .Include(u => u.hebronPayWallet)
                .Include(u => u.subAccount)
                .FirstOrDefault();

            if(user.hebronPayWallet.walletPin != model.currentPin)
            {
                return returnedResponse.ErrorResponse("The current pin passed is incorrect", null);
            }

            
            if (model.newWalletPin != model.confirmNewWalletPin)
            {
                return returnedResponse.ErrorResponse("Pin and Confirm pin do not match", null);
            }

            if (model.newWalletPin == user.hebronPayWallet.walletPin)
            {
                return returnedResponse.ErrorResponse("New pin cannot be same as old pin", null);
            }


            //ensure pin meets the validations
            var validatePin = ValidatePin(model.newWalletPin.ToString());
            if (validatePin.error != null)
            {
                return returnedResponse.ErrorResponse(validatePin.error.message, null);
            }

            user.hebronPayWallet.walletPin = model.newWalletPin;
            user.isPinSet = true;
            await _context.SaveChangesAsync();
            return returnedResponse.CorrectResponse("successfully set pin");


        }


        public async Task<ApiResponse> ForgotPassword(ForgotPasswordModel model)
        {
            ReturnedResponse returnedResponse = new ReturnedResponse();

            // check if there is any user with that email or username
            var user = await userManager.FindByNameAsync(model.email) ?? await userManager.FindByEmailAsync(model.email);

            //if user does not exist, return an error response
            if (user == null)
            {
                return returnedResponse.ErrorResponse("No User exists with that Username or Email", null);
            }


            //ensure that the new password to be set is valid using regexp
            var validPassword = ValidatePassword(model.newPassword);
            if (validPassword.Message == ApiResponseEnum.failure.ToString())
            {
                return returnedResponse.ErrorResponse(validPassword.error.message, null);
            }


            //ensure that the password and confirm passwords match
            if (model.newPassword != model.confirmPassword)
            {
                return returnedResponse.ErrorResponse("Password and Confirm Password do not match", null);
            }


            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, model.newPassword);

            if (result.Succeeded)
            {
                return returnedResponse.CorrectResponse("Successfully changed Passwords");
            }
            return returnedResponse.ErrorResponse(result.Errors.ToString(), null);


        }

        public async Task<ApiResponse> ChangePassword(string username, ChangePasswordModel model)
        {
            ReturnedResponse returnedResponse = new ReturnedResponse();


            // check if there is any user with that email or username
            var user = await userManager.FindByNameAsync(username) ?? await userManager.FindByEmailAsync(username);

            //if user does not exist, return an error response
            if (user == null)
            {
                return returnedResponse.ErrorResponse("No User exists with that Username or Email", null);
            }


            //check if the password used is correct
            bool correctPassword = await userManager.CheckPasswordAsync(user, model.currentPassword);

            //if password is incorrect, return an error response
            if (!correctPassword) return returnedResponse.ErrorResponse("Incorrect Password", null);

            //var changePassword = await ForgotPassword(model.username, model.newPassword, model.confirmPassword);
            var changePassword = await ForgotPassword(new ForgotPasswordModel { confirmPassword = model.confirmPassword, email = username, newPassword = model.newPassword});

            if (changePassword.Message == ApiResponseEnum.failure.ToString())
            {
                return returnedResponse.ErrorResponse(changePassword.error.message, null);
            }

            return returnedResponse.CorrectResponse("Successfully changed Password");


        }


        public ApiResponse ValidatePassword(string password)
        {

            string errorMessage = "";

            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasMiniChars = new Regex(@".{8,}");
            var hasLowerChar = new Regex(@"[a-z]+");
            var hasSymbols = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

            ReturnedResponse returnedResponse = new ReturnedResponse();

            if (!hasLowerChar.IsMatch(password))
            {
                errorMessage = "Password should contain At least one lower case letter";
                return returnedResponse.ErrorResponse(errorMessage, null);
            }
            else if (!hasUpperChar.IsMatch(password))
            {
                errorMessage = "Password should contain At least one upper case letter";
                return returnedResponse.ErrorResponse(errorMessage, null);
            }
            else if (!hasMiniChars.IsMatch(password))
            {
                errorMessage = "Password should not be less than 8 characters";
                return returnedResponse.ErrorResponse(errorMessage, null);
            }
            else if (!hasNumber.IsMatch(password))
            {
                errorMessage = "Password should contain At least one numeric value";
                return returnedResponse.ErrorResponse(errorMessage, null);
            }

            else if (!hasSymbols.IsMatch(password))
            {
                errorMessage = "Password should contain At least one special case characters";
                return returnedResponse.ErrorResponse(errorMessage, null);
            }
            else
            {
                return returnedResponse.CorrectResponse(true);
            }
        }

        public ApiResponse ValidatePin(string pin)
        {
            //for pin to be valid it should be length4, NOT forward consecutive and NOT backward consecutive and NOT repeating

            string errorMessage = "";
            ReturnedResponse returnedResponse = new ReturnedResponse();

            bool isValidPinLength = false;
            bool isForwardConsecutive = false;
            bool isBackwardConsecutive = false;
            bool isNonRepeating = false;

            isValidPinLength = pin.Length == 4;
            if (!isValidPinLength) return returnedResponse.ErrorResponse("Pin must be 4 digits", null);

            isForwardConsecutive = checkPinForwardConsecutive(pin);
            if (isForwardConsecutive) return returnedResponse.ErrorResponse("Pin cannot be 4 consecutive numbers", null);

            isBackwardConsecutive = checkPinBackwardConsecutive(pin);
            if (isBackwardConsecutive) return returnedResponse.ErrorResponse("Pin cannot be 4 consecutive numbers", null);

            isNonRepeating = checkPinNonRepeatingNumber(pin);
            if (!isNonRepeating) return returnedResponse.ErrorResponse("Pin cannot be one digit repeated", null);

            return returnedResponse.CorrectResponse(true);




        }

        public Task<ApiResponse> UpdateUserWallet(string username)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse> getSubAccountBalance(string username)
        {
            ReturnedResponse returnedResponse = new ReturnedResponse();

            try
            {
                var user = await _context.Users.Where(u => u.UserName == username)
               .Include(u => u.hebronPayWallet)
               .Include(u => u.subAccount)
               .FirstAsync();

                var userSubAccount = user.subAccount;
                var userHebronPayWallet = user.hebronPayWallet;


                var flutterwaveResponse = await _flutterwaveServices.getWalletBalance(userSubAccount.account_reference);
                if (flutterwaveResponse.status != FlutterWaveResponseEnum.success.GetEnumDescription())
                {
                    return returnedResponse.ErrorResponse(flutterwaveResponse.message, null);
                }

                GetWalletBalanceModel getWalletBalanceModel = JsonConvert.DeserializeObject<GetWalletBalanceModel>(flutterwaveResponse.data.ToString());
                return returnedResponse.CorrectResponse(getWalletBalanceModel);


            }

            catch (Exception e)
            {
                return returnedResponse.ErrorResponse(e.Message, null);
            }


        }

        /*public async Task<ApiResponse> UpdateUserWallet(string username)
        {
            ReturnedResponse returnedResponse = new ReturnedResponse();

            try
            {
                var user = await _context.Users.Where(u => u.UserName == username)
               .Include(u => u.hebronPayWallet)
               .Include(u => u.subAccount)
               .FirstAsync();

                //user.hebronPayWallet.walletBalance = user.subAccount

            }

            catch(Exception e)
            {
                return returnedResponse.ErrorResponse(e.Message, null);
            }

           
        }

        */


        public bool checkPinForwardConsecutive(string pin)
        {
            for (int i = 0; i < pin.Length - 1; i++)
            {
                int currentDigit = int.Parse(pin[i].ToString());
                int nextDigit = int.Parse(pin[i + 1].ToString());

                if (nextDigit - currentDigit != 1)
                {
                    return false;
                }
            }

            return true;
        }

        public bool checkPinBackwardConsecutive(string pin)
        {
            for (int i = 0; i < pin.Length - 1; i++)
            {
                int currentDigit = int.Parse(pin[i].ToString());
                int nextDigit = int.Parse(pin[i + 1].ToString());

                if (currentDigit - nextDigit != 1)
                {
                    return false;
                }
            }

            return true;
        }

        public bool checkPinNonRepeatingNumber(string pin)
        {
            char firstDigit = pin[0];
            for (int i = 1; i < pin.Length; i++)
            {
                if (pin[i] != firstDigit)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
