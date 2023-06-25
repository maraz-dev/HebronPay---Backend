using AutoMapper;
using HebronPay.Authentication;
using HebronPay.FlutterwaveServices.Interface;
using HebronPay.Model;
using HebronPay.Model.EmailSettings;
using HebronPay.Model.FlutterWave.SubAccout;
using HebronPay.Model.FlutterWave.Transfer;
using HebronPay.Model.RapidAPI;
using HebronPay.Model.Transactions;
using HebronPay.Responses;
using HebronPay.Responses.Enums;
using HebronPay.Services.Interface;
//using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using OfficeOpenXml;
using System.IO;
using System.Net.Mime;
using HebronPay.DTOs;

namespace HebronPay.Services.Implementation
{
    public class TransactionServices :ITransactionServices
    {
        private readonly UserManager<ApplicationUser> userManager;
        private ApplicationDbContext _context;
        private readonly IFlutterwaveServices _flutterwaveServices;
        private readonly EmailSettings _emailSettings;

        public TransactionServices(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IOptions<EmailSettings> emailSettings, IFlutterwaveServices flutterwaveServices)
        {
            this.userManager = userManager;
            _context = context;
            _flutterwaveServices = flutterwaveServices;
            _emailSettings = emailSettings.Value;


        }

        //PLEASEEEEEE DO NOT FORGET TO ADD A CHECK FOR WHETHER THE AMOUNT >= WALLET BALANCE FOR INSUFFICIENT FUNDS
        
        public async Task<ApiResponse> generateTicket(string username, GenerateTicketModel model)
        {

            //this method is to create a hebron pay wallet transaction basiacally

            //PLEASEEEEEE DO NOT FORGET TO ADD A CHECK FOR WHETHER THE AMOUNT >= WALLET BALANCE FOR INSUFFICIENT FUNDS

            ReturnedResponse returnedResponse = new ReturnedResponse();

            if (model.amount <= 250)
            {
                return returnedResponse.ErrorResponse("AMOUNT CANNOT BE LESS THAN ₦250", null);
            }


            //var user = await userManager.FindByNameAsync(username);
            var user = await _context.Users
                .Where(u => u.UserName == username)
                .Include(u=>u.hebronPayWallet)
                .FirstAsync();
            var userHebronPayWallet = user.hebronPayWallet;

            if(model.amount >= userHebronPayWallet.walletBalance)
            {
                return returnedResponse.ErrorResponse("INSUFFICIENT BALANCE FOR THIS TRANSACTION", null);
            }

            try
            {
                HebronPayTransaction transaction = new HebronPayTransaction
                {
                    amount = model.amount,
                    reference = generateRandomString(20),
                    date = getCurrentdate(),
                    time = getCurrentTime(),
                    description = model.description,
                    type = HebronPayTransactionTypeEnum.pending.GetEnumDescription(),
                    hebronPayWalletId = userHebronPayWallet.id,
                    hebronPayWallet = userHebronPayWallet,
                };
                
                userHebronPayWallet.walletBalance -= model.amount;
                
                await _context.HebronPayTransactions.AddAsync(transaction);
                
                await _context.SaveChangesAsync();

                //userHebronPayWallet.hebronPayTransactions.Add(transaction);

                return returnedResponse.CorrectResponse(transaction);

            }

            catch (Exception e)
            {
                return returnedResponse.ErrorResponse(e.Message, null);
            }
            
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

        public async Task<ApiResponse> getPendingTransactions(string username)
        {
            ReturnedResponse returnedResponse = new ReturnedResponse();

            try
            {
                //get the user's details and the user's wallet details.
                var user = await _context.Users
                                .Where(u => u.UserName == username)
                                .Include(u => u.hebronPayWallet)
                                .FirstAsync();
                var userHebronPayWallet = user.hebronPayWallet;


                //use the user's wallet details to get pending transactions on the wallet.
                var transactions = await _context.HebronPayTransactions.Where(t => t.type == HebronPayTransactionTypeEnum.pending.GetEnumDescription() && t.hebronPayWalletId == userHebronPayWallet.id).ToListAsync();

                return returnedResponse.CorrectResponse(transactions);
            }
            catch(Exception e)
            {
                return returnedResponse.ErrorResponse(e.Message, null);
            }


        }

        public async Task<ApiResponse> deleteTicket(string reference)
        {
            //PLEASEEEE REFUND THE MONEY BACK TO THE USER'S HEBRON PAY WALLET
            ReturnedResponse returnedResponse = new ReturnedResponse();
            var ticket = await _context.HebronPayTransactions.Where(t => t.reference == reference && t.type == HebronPayTransactionTypeEnum.pending.GetEnumDescription())
                .OrderBy(t=>t.id).LastAsync();

            if(ticket == null)
            {
                return returnedResponse.ErrorResponse("TICKET DOES NOT EXIST", null);

            }

            try
            {
                var user = await _context.Users.Where(u => u.hebronPayWalletId == ticket.hebronPayWalletId)
                    .Include(u=>u.hebronPayWallet)
                    .FirstAsync();
                user.hebronPayWallet.walletBalance += ticket.amount;

                _context.HebronPayTransactions.Remove(ticket);
                await _context.SaveChangesAsync();
                return returnedResponse.CorrectResponse("successfully deleted ticket");
            }
            catch(Exception e)
            {
                return returnedResponse.ErrorResponse(e.Message, null);
            }
        }

        public async Task<ApiResponse> getSubAccountDetails(string username)
        {
            ReturnedResponse returnedResponse = new ReturnedResponse();

            try
            {
                var user = await _context.Users.Where(u => u.UserName == username).Include(u => u.subAccount).Include(u => u.hebronPayWallet).FirstAsync();
                var userSubAccount = user.subAccount;
                return returnedResponse.CorrectResponse(userSubAccount);
            }
            catch(Exception e)
            {
                return returnedResponse.ErrorResponse(e.Message, null);
            }


        
        
        }

        public async Task<ApiResponse> getTransactionDetails(string username, string reference)
        {

            //this method retuns the transaction details AND the details of the sub account of the user that will be debited for the transaction
            ReturnedResponse returnedResponse = new ReturnedResponse();

            var mapper = new Mapper(MapperConfig.GetMapperConfiguration());



            try
            {
                //var user = await _context.Users.Where(u => u.UserName == username).Include(u => u.subAccount).Include(u => u.hebronPayWallet).FirstAsync();
                //var hebronPayWallet = user.hebronPayWallet;
                //var userSubAccount = user.subAccount;

                var transaction = await _context.HebronPayTransactions
                    .Where(t=> t.reference == reference && t.type == HebronPayTransactionTypeEnum.pending.GetEnumDescription())
                    .OrderBy(t => t.id).LastAsync();
                if (transaction == null)
                {
                    return returnedResponse.ErrorResponse("No such transaction exists",null);
                }

                var userToBeDebited = await _context.Users.Where(u => u.hebronPayWalletId == transaction.hebronPayWalletId).FirstAsync();



                var getTransactionDTO = mapper.Map<GetTransactionDTO>(transaction);
                getTransactionDTO.username = $"{userToBeDebited.FirstName} {userToBeDebited.LastName}";

                //var transactionResponse = mapper.Map<GetTransactionResponse>(transaction);

                /* transactionResponse.flutterwaveSubAccountId = userSubAccount.flutterwaveSubAccountId;
                 transactionResponse.account_reference = userSubAccount.account_reference;
                 transactionResponse.account_name = userSubAccount.account_name;
                 transactionResponse.barter_id = userSubAccount.barter_id;
                 transactionResponse.email = userSubAccount.email;
                 transactionResponse.mobilenumber = userSubAccount.mobilenumber;
                 transactionResponse.country = userSubAccount.country;
                 transactionResponse.nuban = userSubAccount.nuban;
                 transactionResponse.bank_name = userSubAccount.bank_name;
                 transactionResponse.bank_code = userSubAccount.bank_code;
                 transactionResponse.status = userSubAccount.status;*/




                return returnedResponse.CorrectResponse(getTransactionDTO);

            }
            catch (Exception e)
            {
                return returnedResponse.ErrorResponse(e.Message, null);
            }

        }

        public async Task<ApiResponse> initiateTransfer(InitiateTransferRequest model)
        {
            ReturnedResponse returnedResponse = new ReturnedResponse();

            try
            {

                model.debit_currency = CurrencyEnum.NGN.GetEnumDescription();
                model.currency = CurrencyEnum.NGN.GetEnumDescription();

                var flutterwaveResponse = await _flutterwaveServices.initiateTransfer(model);

                if (flutterwaveResponse.status != FlutterWaveResponseEnum.success.GetEnumDescription())
                {
                    return returnedResponse.ErrorResponse(flutterwaveResponse.message, null);
                }

                InitiateTransferResponse transferData = JsonConvert.DeserializeObject<InitiateTransferResponse>(flutterwaveResponse.data.ToString());
                return returnedResponse.CorrectResponse(transferData);

            }

            catch (Exception e)
            {
                return returnedResponse.ErrorResponse(e.Message, null);
            }



        }

        public async Task<ApiResponse> fundWallet(string username, FundWalletModel model)
        {
            ReturnedResponse returnedResponse = new ReturnedResponse();

            if(model.amount <= 200)
            {
                return returnedResponse.ErrorResponse("Cannot fund less than ₦200", null);
            }

            //var user = await userManager.FindByNameAsync(username);
            var user = await _context.Users
                .Where(u => u.UserName == username)
                .Include(u => u.hebronPayWallet)
                .FirstAsync();
            var userHebronPayWallet = user.hebronPayWallet;

            userHebronPayWallet.walletBalance += model.amount;

            await _context.SaveChangesAsync();

            HebronPayTransaction transaction = new HebronPayTransaction
            {
                amount = model.amount,
                reference = generateRandomString(20),
                date = getCurrentdate(),
                time = getCurrentTime(),
                description = $"funding of {model.amount}",
                type = HebronPayTransactionTypeEnum.credit.GetEnumDescription(),
                hebronPayWalletId = userHebronPayWallet.id,
                hebronPayWallet = userHebronPayWallet,
            };
            await _context.HebronPayTransactions.AddAsync(transaction);
            await _context.SaveChangesAsync();

            return returnedResponse.CorrectResponse("successfully funded wallet");

        }

        public async Task<ApiResponse> confirmTicketPayment(string receiverUsername, HebronPayTransaction pendingTransaction)
        {

            ReturnedResponse returnedResponse = new ReturnedResponse();

            try
            {
                var receiverUser = await _context.Users
                .Where(u => u.UserName == receiverUsername)
                .Include(u => u.hebronPayWallet)
                .FirstAsync();

                var receiverUserHebronPayWallet = receiverUser.hebronPayWallet;

                if(receiverUserHebronPayWallet.id == pendingTransaction.hebronPayWallet.id)
                {
                    return returnedResponse.ErrorResponse("You cannot confirm this ticket because you initiated the transaction yourself", null);
                }



                var newpendingTransaction = await _context.HebronPayTransactions.Where(t => t.id == pendingTransaction.id && t.reference == pendingTransaction.reference && t.type == HebronPayTransactionTypeEnum.pending.GetEnumDescription()).FirstAsync();

                if(newpendingTransaction == null)
                {
                    return returnedResponse.ErrorResponse("This transaction does not exist", null);

                }

                newpendingTransaction.type = HebronPayTransactionTypeEnum.debit.GetEnumDescription();
                newpendingTransaction.time = getCurrentTime();
                newpendingTransaction.date = getCurrentdate();

                HebronPayTransaction transaction = new HebronPayTransaction
                {
                    amount = pendingTransaction.amount,
                    reference = generateRandomString(20),
                    date = getCurrentdate(),
                    time = getCurrentTime(),
                    description = pendingTransaction.description,
                    type = HebronPayTransactionTypeEnum.credit.GetEnumDescription(),
                    hebronPayWalletId = receiverUserHebronPayWallet.id,
                    hebronPayWallet = receiverUserHebronPayWallet,
                };

                receiverUserHebronPayWallet.walletBalance += pendingTransaction.amount;

                await _context.HebronPayTransactions.AddAsync(transaction);
                await _context.SaveChangesAsync();

                return returnedResponse.CorrectResponse("Successfully completed transaction");
            }
            catch (Exception e)
            {
                return returnedResponse.ErrorResponse(e.Message, null);
            }
            


        }

        public async Task<ApiResponse> getAllTransactions()
        {
            //throw new NotImplementedException();
            
            ReturnedResponse returnedResponse = new ReturnedResponse();
            
            var allTransactions = await _context.HebronPayTransactions.ToListAsync();

            return returnedResponse.CorrectResponse(allTransactions);
        }

        public async Task<ApiResponse> getUsersTransactions(string username)
        {
            ReturnedResponse returnedResponse = new ReturnedResponse();

            try
            {
                var user = await _context.Users
                .Where(u => u.UserName == username)
                .Include(u => u.hebronPayWallet)
                .FirstAsync();
                var userHebronPayWallet = user.hebronPayWallet;

                var allTransactions = await _context.HebronPayTransactions.Where(t => t.hebronPayWalletId == userHebronPayWallet.id && t.type != HebronPayTransactionTypeEnum.pending.GetEnumDescription())
                    .ToListAsync();
                allTransactions.Reverse();

                return returnedResponse.CorrectResponse(allTransactions);

            }
            catch(Exception e)
            {
                return returnedResponse.ErrorResponse(e.Message, null);
            }
            
        }

        public async Task<ApiResponse> resolveBankAccount(ResolveAccountDetailsRequest model)
        {

            ReturnedResponse returnedResponse = new ReturnedResponse();

            try
            {
                var flutterwaveResponse = await _flutterwaveServices.getBankAccountDetails(model);

                if (flutterwaveResponse.status != FlutterWaveResponseEnum.success.GetEnumDescription())
                {
                    return returnedResponse.ErrorResponse(flutterwaveResponse.message, null);
                }
                ResolveAccountDetailsResponse userBankAccount = JsonConvert.DeserializeObject<ResolveAccountDetailsResponse>(flutterwaveResponse.data.ToString());
                return returnedResponse.CorrectResponse(userBankAccount);
            }
            catch(Exception e)
            {
                return returnedResponse.ErrorResponse(e.Message, null);

            }


        }

        public async Task<ApiResponse> getAllBanks()
        {
            ReturnedResponse returnedResponse = new ReturnedResponse();

            try
            {
                var flutterwaveResponse = await _flutterwaveServices.getBanks();

                if (flutterwaveResponse.status != FlutterWaveResponseEnum.success.GetEnumDescription())
                {
                    return returnedResponse.ErrorResponse(flutterwaveResponse.message, null);
                }
                List<Bank> listOfBanks = JsonConvert.DeserializeObject<List<Bank>>(flutterwaveResponse.data.ToString());
                return returnedResponse.CorrectResponse(listOfBanks);
            }
            catch (Exception e)
            {
                return returnedResponse.ErrorResponse(e.Message, null);

            }

        }

        public async Task<ApiResponse> withdraw(string username, WithdrawModel model)
        {
            ReturnedResponse returnedResponse = new ReturnedResponse();

            if (model.amount <= 1000)
            {
                return returnedResponse.ErrorResponse("Cannot withdraw less than ₦1000", null);
            }

            //var user = await userManager.FindByNameAsync(username);
            var user = await _context.Users
                .Where(u => u.UserName == username)
                .Include(u => u.hebronPayWallet)
                .FirstAsync();
            var userHebronPayWallet = user.hebronPayWallet;

            userHebronPayWallet.walletBalance -= model.amount;

            await _context.SaveChangesAsync();

            HebronPayTransaction transaction = new HebronPayTransaction
            {
                amount = model.amount,
                reference = generateRandomString(20),
                date = getCurrentdate(),
                time = getCurrentTime(),
                description = $"withdrawal of ₦{model.amount} to {model.account_name} for {model.narration??""}",
                type = HebronPayTransactionTypeEnum.debit.GetEnumDescription(),
                hebronPayWalletId = userHebronPayWallet.id,
                hebronPayWallet = userHebronPayWallet,
            };
            await _context.HebronPayTransactions.AddAsync(transaction);
            await _context.SaveChangesAsync();

            return returnedResponse.CorrectResponse("successfully withdrew from wallet");

        }



        public async Task<ApiResponse> sendEmailAsync(string email, string subject, string message)
        {
            ReturnedResponse returnedResponse = new ReturnedResponse();
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excelPackage = new ExcelPackage();
                var worksheet = excelPackage.Workbook.Worksheets.Add("Sheet1");
                worksheet.Cells["A1"].Value = "Hello, World!";
                byte[] excelBytes = excelPackage.GetAsByteArray();
           
                var senderEmail = _emailSettings.emailAddress;
                var senderPassword = _emailSettings.password;

                var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(senderEmail, senderPassword)
                };

                MailMessage mailMessage = new MailMessage(senderEmail, email);
                mailMessage.Subject = subject;
                mailMessage.Body = message;
                Attachment attachment = new Attachment(new MemoryStream(excelBytes), "ExcelFile.xlsx", MediaTypeNames.Application.Octet);
                mailMessage.Attachments.Add(attachment);

                await client.SendMailAsync(mailMessage);

                return returnedResponse.CorrectResponse("successfully sent message");

            }

            catch (Exception e)
            {
                return returnedResponse.ErrorResponse(e.Message, null);
            }
            


        }

        public async Task<ApiResponse> generateEod(string username)
        {
            ReturnedResponse returnedResponse = new ReturnedResponse();
            
            try
            {
                var todayDate = DateTime.Today.ToString("dd-MM-yyyy");
                //GET USER'S TRANSACTIONS FOR THE DAY
                var user = await _context.Users
                .Where(u => u.UserName == username)
                .Include(u => u.hebronPayWallet)
                .FirstAsync();
                var userHebronPayWallet = user.hebronPayWallet;

                var allTransactions = await _context.HebronPayTransactions.Where(t => t.hebronPayWalletId == userHebronPayWallet.id).ToListAsync();
                
                List<HebronPayTransaction> usersDailyTransactions = new List<HebronPayTransaction>();

                foreach(var transaction in allTransactions)
                {
                    if(transaction.date == todayDate && transaction.type != HebronPayTransactionTypeEnum.pending.GetEnumDescription())
                    {
                        usersDailyTransactions.Add(transaction);
                    }
                    

                }

                var userCreditTransactions = usersDailyTransactions.Where(t => t.type == HebronPayTransactionTypeEnum.credit.GetEnumDescription()).ToList();
                var totalNumberOfCreditTransactions = userCreditTransactions.Count();
                var totalSumOfCreditTransactions = userCreditTransactions.Sum(t => t.amount);


                var usersDebitTransactions = usersDailyTransactions.Where(t => t.type == HebronPayTransactionTypeEnum.debit.GetEnumDescription()).ToList();
                var totalNumberOfDebitTransactions = usersDebitTransactions.Count();
                var totalSumOfDebitTransactions = usersDebitTransactions.Sum(t => t.amount);

                var totalNumberOfTransactions = usersDailyTransactions.Count();

                var netSpend = totalSumOfCreditTransactions - totalSumOfDebitTransactions;
                //GENERATE EXCEL FILE FOR THE TRANSACTIONS

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excelPackage = new ExcelPackage();
                var worksheet = excelPackage.Workbook.Worksheets.Add($"EOD for {user.UserName} on {todayDate}");
                worksheet.Cells[1, 1].Value = "Description";
                worksheet.Cells[1, 2].Value = "Amount";
                worksheet.Cells[1, 3].Value = "Reference";
                worksheet.Cells[1, 4].Value = "Type";
                worksheet.Cells[1, 5].Value = "Date";
                worksheet.Cells[1, 6].Value = "Time";

                int row = 2; // Start from the second row
                foreach (var transaction in usersDailyTransactions)
                {
                    worksheet.Cells[row, 1].Value = transaction.description;
                    worksheet.Cells[row, 2].Value = transaction.amount;
                    worksheet.Cells[row, 3].Value = transaction.reference;
                    worksheet.Cells[row, 4].Value = transaction.type;
                    worksheet.Cells[row, 5].Value = transaction.date;
                    worksheet.Cells[row, 6].Value = transaction.time;

                    row++;
                }
                row++;
                worksheet.Cells[row, 1].Value = "Total Number of credit transactions"; worksheet.Cells[row, 2].Value = totalNumberOfCreditTransactions;
                
                row++;
                worksheet.Cells[row, 1].Value = "Total worth of credit transactions"; worksheet.Cells[row, 2].Value = totalSumOfCreditTransactions;

                row+=2;
                worksheet.Cells[row, 1].Value = "Total number of debit transactions"; worksheet.Cells[row, 2].Value = totalNumberOfDebitTransactions;

                row++;
                worksheet.Cells[row, 1].Value = "Total worth of debit transactions"; worksheet.Cells[row, 2].Value = totalSumOfDebitTransactions;

                row += 2;
                worksheet.Cells[row, 1].Value = "Total number of transactions"; worksheet.Cells[row, 2].Value = totalNumberOfTransactions;

                row++;
                worksheet.Cells[row, 1].Value = "Total net spend"; worksheet.Cells[row, 2].Value = netSpend;



                byte[] excelBytes = excelPackage.GetAsByteArray();

                //send the excel file as an email 
                var senderEmail = _emailSettings.emailAddress;
                var senderPassword = _emailSettings.password;

                var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(senderEmail, senderPassword)
                };

                MailMessage mailMessage = new MailMessage(senderEmail, user.Email);
                mailMessage.Subject = "End of Day Report";
                mailMessage.Body = $"Hello {user.UserName}, Please kindly find attached your End of Day Transactions for today: {todayDate}. Thank you for using Hebron Pay";
                Attachment attachment = new Attachment(new MemoryStream(excelBytes), "ExcelFile.xlsx", MediaTypeNames.Application.Octet);
                mailMessage.Attachments.Add(attachment);

                await client.SendMailAsync(mailMessage);

                return returnedResponse.CorrectResponse("successfully sent message");




            }
            catch (Exception e)
            {
                return returnedResponse.ErrorResponse(e.Message, null);
            }
        }

        public string getCurrentTime()
        {
            // Get the current time in your current location

            DateTime currentTime = DateTime.Now;

            // Get the time zone of your current location
            TimeZoneInfo timeZone = TimeZoneInfo.Local;

            // Convert the current time to the time zone of your current location
            DateTime currentLocalTime = TimeZoneInfo.ConvertTime(currentTime, timeZone);

            var newcurrentTime = currentLocalTime.ToString("hh:mm tt");

            return newcurrentTime;

        }

        public string getCurrentdate()
        {
            // Get the current time in your current location

            DateTime currentTime = DateTime.Now;

            // Get the time zone of your current location
            TimeZoneInfo timeZone = TimeZoneInfo.Local;

            // Convert the current time to the time zone of your current location
            DateTime currentLocalTime = TimeZoneInfo.ConvertTime(currentTime, timeZone);
            var newcurrentDate = currentLocalTime.ToString("dd-MM-yyyy");

            return newcurrentDate;

        }

        public async Task<ApiResponse> getUsersPendingTransactions(string username)
        {
            ReturnedResponse returnedResponse = new ReturnedResponse();
            try
            {
                var user = await _context.Users
                .Where(u => u.UserName == username)
                .Include(u => u.hebronPayWallet)
                .FirstAsync();
                var userHebronPayWallet = user.hebronPayWallet;

                var allTransactions = await _context.HebronPayTransactions
                    .Where(t => t.hebronPayWalletId == userHebronPayWallet.id && t.type == HebronPayTransactionTypeEnum.pending.GetEnumDescription())
                    .ToListAsync();
                allTransactions.Reverse();

                return returnedResponse.CorrectResponse(allTransactions);

            }
            catch (Exception e)
            {
                return returnedResponse.ErrorResponse(e.Message, null);
            }
        }
    }
}
