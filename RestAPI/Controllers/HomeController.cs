using DataCommon;
using DataCommon.Models.Documents;
using DataCommon.Models.Utils;
using DataService.Repository.Interfaces.Documents;
using DataService.Services.Interfaces.Documents;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IDbClient _dbClient;
        private readonly IIgnoredRepository _ignoredRepository;
        private readonly ITagService _tagService;
        private readonly IPostRepository _postRepository;
        private readonly IUserRepository _userRepository;


        public HomeController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IDbClient dbClient, IIgnoredRepository ignoredRepository, ITagService tagService, IPostRepository postRepository, IUserRepository userRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbClient = dbClient;
            _ignoredRepository = ignoredRepository;
            _tagService = tagService;
            _postRepository = postRepository;
            _userRepository = userRepository;
        }

        [HttpGet]
        public string Hi(){
            return "Hello World";
        }

        // GET: api/<HomeController>
        [HttpGet("init")]
        public async Task<object> Get()
        {
            await _roleManager.CreateAsync(new ApplicationRole(UserRoles.Admin));
            await _roleManager.CreateAsync(new ApplicationRole(UserRoles.User));

            var user = new ApplicationUser
            {
                UserName = "Kubcia98",
                Email = "jakub.i.iwaniuk@gmail.com",
                EmailConfirmed = true,
                Name = "Jakub",
                Surename = "Iwaniuk",
                AboutMe = "Nothing",
                Photo = "test.jpg",
                BackgroundPhoto = "test2.jpg",
                Nick = "Hargiljusz",
                PhoneNumber = "111222333",
                PhoneNumberConfirmed = true
            };
            var res1 = await _userManager.CreateAsync(user, "Test1234!");
            await _userManager.AddToRoleAsync(user,UserRoles.User);
            await _ignoredRepository.Add(new Ignored
            {
                CreatedAt = DateTime.Now,
                UserId = user.Id.ToString()
            });

            var user2 = new ApplicationUser
            {
                UserName = "User1",
                Email = "user@gmail.com",
                EmailConfirmed = true,
                Name = "Adam",
                Surename = "Nowak",
                AboutMe = "Nothing",
                Photo = "test.jpg",
                BackgroundPhoto = "test2.jpg",
                Nick = "UserNivk",
                PhoneNumber = "111222333",
                PhoneNumberConfirmed = true
            };
            await _userManager.CreateAsync(user2, "Test1234!");
            await _userManager.AddToRoleAsync(user2, UserRoles.User);
            await _ignoredRepository.Add(new Ignored
            {
                CreatedAt = DateTime.Now,
                UserId = user2.Id.ToString()
            });

            var admin = new ApplicationUser
            {
                UserName = "Admin",
                Email = "admin@gmail.com",
                EmailConfirmed = true,
                Name = "Adam",
                Surename = "Nowak",
                AboutMe = "Nothing",
                Photo = "test.jpg",
                BackgroundPhoto = "test2.jpg",
                Nick = "AdminNick",
                PhoneNumber = "111222333",
                PhoneNumberConfirmed = true
            };
            await _userManager.CreateAsync(admin, "Test1234!");
            await _userManager.AddToRoleAsync(admin, UserRoles.Admin);
            await _ignoredRepository.Add(new Ignored
            {
                CreatedAt = DateTime.Now,
                UserId = admin.Id.ToString()
            });

            await InitData();

            return new { ok = "ok" };
        }

        [HttpGet("file/{fVar}/{sVar}")]
        public async Task<object> GetFile(string fVar,string sVar)
        {
            await Task.Delay(1);
            return new
            {
                firstVariable = fVar,
                secondVariable = sVar
            };
        }

        private async Task InitData()
        {
            await Task.Delay(10);
        }
    }
}
