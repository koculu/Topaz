using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Bogus;
using Microsoft.ClearScript.V8;
using System;
using System.Collections.Generic;

namespace Tenray.Topaz.Benchmark;

[SimpleJob(RunStrategy.ColdStart, iterationCount: 10)]
public class Benchmark5
{
    public string Code = @"
var list = []
for (const item of model.Profiles) {
    let x = {}
    x.Name = item.Name + 1
    x.Address = item.Address + 1
    x.Bio = item.Bio + 1
    x.City = item.City + 1
    x.Country = item.Country + 1
    x.Email = item.Email + 1
    x.Phone = item.Phone + 1
    list.push(x)
}
model.List = list
";

    public Faker<UserProfile> UserProfileFaker { get; private set; }
    public List<UserProfile> FakeUserProfiles { get; private set; }

    public Benchmark5()
    {
        SetupFakeData();
    }

    private void SetupFakeData()
    {
        Randomizer.Seed = new Random(8675309);
        SetupUserProfileFaker();
        var len = 100000;
        var profiles = new List<UserProfile>(len);
        for (var i = 0; i < len; ++i)
        {
            profiles.Add(UserProfileFaker.Generate());
        }
        FakeUserProfiles = profiles;
    }

    private void SetupUserProfileFaker()
    {
        UserProfileFaker = new Faker<UserProfile>()
            .RuleFor(u => u.Name, (f, u) => f.Name.FullName())
            .RuleFor(u => u.Address, (f, u) => f.Address.FullAddress())
            .RuleFor(u => u.Bio, (f, u) => f.Lorem.Sentences(3))
            .RuleFor(u => u.City, (f, u) => f.Address.City())
            .RuleFor(u => u.Country, (f, u) => f.Address.Country())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email())
            .RuleFor(u => u.Phone, (f, u) => f.Phone.PhoneNumber());
    }


    public class UserProfile
    {
        public string Name { get; set; }

        public int Followers { get; set; }

        public string Address { get; set; }

        public string Bio { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }
    }

    public class Model
    {
        public List<UserProfile> Profiles;

        public object List;

        public Model(List<UserProfile> profiles)
        {
            Profiles = profiles;
        }
    }

    [Benchmark]
    public void RunTopaz()
    {
        var topazEngine = new TopazEngine(new TopazEngineSetup
        {
            IsThreadSafe = false
        });
        var model = new Model(FakeUserProfiles);
        topazEngine.SetValue("model", model);
        topazEngine.ExecuteScript(Code);
    }

    //[Benchmark]
    public void RunV8Engine()
    {
        var v8Engine = new V8ScriptEngine();
        var model = new Model(FakeUserProfiles);
        v8Engine.AddHostObject("model", model);
        v8Engine.Execute(Code);
    }

    [Benchmark]
    public void RunJint()
    {
        var jintEngine = new Jint.Engine();
        var model = new Model(FakeUserProfiles);
        jintEngine.SetValue("model", model);
        jintEngine.Execute(Code);
    }
}
