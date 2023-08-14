using Bogus;
using Microsoft.AspNetCore.Mvc;
using task5.Models;

namespace task5.Controllers
{
    public class UserController : Controller
    {
        private static int number = 0;
        private Random random;

        public UserController()
        {
            random = new Random();
        }

        [HttpGet]
        public JsonResult RefreshNumberValue()
        {
            number = 0;
            return Json(new { success = true });
        }

        public IActionResult Index()
        {
            var users = GenerateNewUsers();
            return View(users);
        }

        [HttpGet]
        public IActionResult GetNewUsers(string region, int numOfUsers, int seed, double errorRate = 0)
        {
            random = new Random(seed);
            var users = GenerateNewUsers(region, numOfUsers, seed);
            if (errorRate > 1000) errorRate = 1000;
            if (errorRate != 0) AddErrorsToUsersData(users, region, errorRate);
            return Json(users);
        }

        private List<User> GenerateNewUsers(string region = "en", int num = 20, int seed = 0)
        {
            var faker = new Faker(region)
            {
                Random = new Randomizer(seed)
            };
            var addressFormats = GetAddressFormats(faker);
            return GenerateUsersList(faker,num,addressFormats);
        }

        private List<User> GenerateUsersList(Faker faker, int num, Func<string>[] addressFormats)
        {
            var users = new List<User>(num);
            for (int i = 0; i < num; i++)
            {
                var user = CreateUserWithAddress(faker, addressFormats);
                users.Add(user);
            }
            return users;
        }

        private Func<string>[] GetAddressFormats(Faker faker)
        {
            return new Func<string>[]
            {
                () => $"{faker.Address.ZipCode()}, {faker.Address.City()}, {faker.Address.StreetAddress()},{faker.Address.BuildingNumber()},{random.Next(1, 100)}",
                () => $"{faker.Address.City()}, {faker.Address.StreetAddress()}, {faker.Address.BuildingNumber()}",
                () => $"{faker.Address.ZipCode()}, {faker.Address.StreetAddress()}, {faker.Address.BuildingNumber()}",
                () => $"{faker.Address.City()}, {faker.Address.StreetAddress()}, {faker.Address.BuildingNumber()}"
            };
        }

        private User CreateUserWithAddress(Faker faker, Func<string>[] addressFormats)
        {
            var addressFormatIndex = random.Next(addressFormats.Length);
            var addressFormat = addressFormats[addressFormatIndex];
            var address = addressFormat();
            return GetNewUser(faker, address);
        }

        private User GetNewUser(Faker faker, string address)
        {
            number++;
            return new User()
            {
                Number = number,
                Id = Guid.NewGuid().ToString(),
                Name = faker.Name.FullName(),
                Address = address,
                PhoneNumber = faker.Phone.PhoneNumber()
            };
        }

        private void AddErrorsToUsersData(List<User> users, string region, double errorRate)
        {
            for (int i = 0; i < users.Count; i++)
            {
                int wholeErrors = (int)Math.Floor(errorRate);
                double fractionalError = errorRate - wholeErrors;
                AddErrorsToUserData(users[i], region, wholeErrors, fractionalError);
            }
        }

        private void AddErrorsToUserData(User user, string region, int wholeErrors, double fractionalError)
        {
            for (int j = 0; j < wholeErrors; j++)
            {
                AddErrorToUserData(user, random, region);
                if (random.NextDouble() < fractionalError)
                {
                    AddErrorToUserData(user, random, region);
                }
            }
        }

        private void AddErrorToUserData(User user, Random random, string region)
        {
            int errorNumber = random.Next(3);
            switch (errorNumber)
            {
                case 0:
                    DeleteRandomSymbol(user, random);
                    break;
                case 1:
                    AddRandomSymbol(user, random, region);
                    break;
                case 2:
                    SwapAdjacentSymbols(user, random);
                    break;
            }
        }

        private void DeleteRandomSymbol(User user, Random random)
        {
            int fieldChoice = random.Next(3);
            switch (fieldChoice)
            {
                case 0: 
                    user.Name = DeleteRandomSymbolInField(user.Name, random);
                    break;
                case 1: 
                    user.Address = DeleteRandomSymbolInField(user.Address, random);
                    break;
                case 2: 
                    user.PhoneNumber = DeleteRandomSymbolInField(user.PhoneNumber, random);
                    break;
            }
        }

        private string DeleteRandomSymbolInField(string input, Random random)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            int indexToRemove = random.Next(input.Length); 
            return input.Remove(indexToRemove, 1); 
        }

        private void AddRandomSymbol(User user, Random random, string region)
        {
            int fieldChoice = random.Next(3);
            switch (fieldChoice)
            {
                case 0:
                    user.Name = AddRandomSymbToField(user.Name, random, region);
                    break;
                case 1:
                    user.Address = AddRandomSymbToField(user.Address, random,region);
                    break;
                case 2:
                    user.PhoneNumber = AddRandomSymbToField(user.PhoneNumber, random, region);
                    break;
            }
        }

        private string AddRandomSymbToField(string input, Random random, string region)
        {
            string localLettersSet = GetLocalLettersSet(region);
            char randomChar = localLettersSet[random.Next(localLettersSet.Length)];
            int indexToInsert = random.Next(input.Length + 1);
            return input.Insert(indexToInsert, randomChar.ToString());
        }

        private string GetLocalLettersSet(string region)
        {
            string res = "";
            switch (region)
            {
                case "en":
                    res = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                    break;
                case "ru":
                    res = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюя0123456789";
                    break;
                case "ge":
                    res = "აბგდევზთიკლმნოპჟრსტუფქღყშჩცძწჭხჯჰ0123456789";
                    break;
            }
            return res;
        }

        private void SwapAdjacentSymbols(User user, Random random)
        {
            int fieldChoice = random.Next(3);
            switch (fieldChoice)
            {
                case 0:
                    user.Name = SwapAdjacentSymbolsInField(user.Name, random);
                    break;
                case 1:
                    user.Address = SwapAdjacentSymbolsInField(user.Address, random);
                    break;
                case 2:
                    user.PhoneNumber = SwapAdjacentSymbolsInField(user.PhoneNumber, random);
                    break;
            }
        }

        private string SwapAdjacentSymbolsInField(string input, Random random)
        {
            if (input.Length < 2) return input;
            int indexToSwap = random.Next(input.Length - 1);
            var inputChars = input.ToCharArray();
            SwapTwoChars(inputChars, indexToSwap, indexToSwap + 1);
            return new string(inputChars);
        }

        private void SwapTwoChars(char[] input, int index1, int index2)
        {
            char temp = input[index1];
            input[index1] = input[index2];
            input[index2] = temp;
        }
    }
}
