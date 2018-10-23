using LightORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation
{
  class Program
  {
    static void Main(string[] args)
    {
      DbContext dbCon = new DbContext();

      // List
      var users = dbCon.SelectQuery("select * from user");
      foreach (var user in users) {
        Console.WriteLine("user.email : " + user.email);
      }

      // Single
      var usersWithEntity = dbCon.SelectQuery<LightORM.Entities.User>("select email,password from user where email = @email", new { email = "murat.aslan@live.com" }).FirstOrDefault();
      Console.WriteLine("usersWithEntity.email : " + usersWithEntity.email);

      // Insert
      bool result = dbCon.InsertData(new LightORM.Entities.User
      {
        email = "abc@test.com",
        password = "1234"
      });

      //Insert Range
      var userList = new List<LightORM.Entities.User>();
      userList.Add(new LightORM.Entities.User
      {
        email = "test1@test.com",
        password = "test1"
      });
      userList.Add(new LightORM.Entities.User
      {
        email = "test2@test.com",
        password = "test2"
      });
      bool resultRange = dbCon.InsertDataIgnoreRange(userList);

      Console.ReadLine();
    }
  }
}