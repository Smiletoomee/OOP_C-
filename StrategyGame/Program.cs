using System;
using System.Collections.Generic;
using System.Linq;

namespace StrategyGame
{
    public interface IGameUnit
    {
        string GetName();
        IGameUnit Clone();
        
    }

public sealed class GameEngineSettings
    {
        private static GameEngineSettings _instance;
        private static readonly object _lock = new object();

        public string Language { get; set; } = "PL";
        public int MaxUnits { get; set; } = 100;

        private GameEngineSettings()
        {
            if (_instance != null)
            {
                throw new InvalidOperationException("Instancja Singletona już istnieje! Użyj GetInstance().");
            }
        }
        
        public static GameEngineSettings GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new GameEngineSettings();
                    }
                }
            }

            return _instance;
        }
    }

public interface IInfantaryUnit : IGameUnit
    {
        string TeamUp(IGameUnit partner)
        {
            return $"{GetName()} osłania jednostkę: {partner.GetName()}";
        } 
    }

public interface IMechanicalUnit : IGameUnit
    {
        string TeamUp(IGameUnit partner)
        {
            return $"{GetName()} osłania jednostkę: {partner.GetName()}";
        } 
    }

public interface IUnitFactory
    {
        IInfantaryUnit CreateInfantary();
        IMechanicalUnit CreateMechanical();
    }

public class Knight : IInfantaryUnit
    {
        public string GetName() => "Rycerz Królestwa";
        public IGameUnit Clone() => (IGameUnit)this.MemberwiseClone();
    }

public class Archer : IMechanicalUnit
    {
        public string GetName() => "Łucznik Królestwa";
        public IGameUnit Clone() => (IGameUnit)this.MemberwiseClone();
    }

public class NorthernKingdomFactory : IUnitFactory
    {
        public IInfantaryUnit CreateInfantary() => new Knight();
        public IMechanicalUnit CreateMechanical() => new Archer();
    }

public class DesertWarrior : IInfantaryUnit
    {
        public string GetName() => "Pustynny Wojownik";
        public IGameUnit Clone() => (IGameUnit)this.MemberwiseClone();
    }

public class BattleScorpion : IMechanicalUnit
    {
        public string GetName() => "Skorpion Bojowy";
        public IGameUnit Clone() => (IGameUnit)this.MemberwiseClone();
    
    }

public class DesertEmpireFactory : IUnitFactory
    {
        public IInfantaryUnit CreateInfantary() => new DesertWarrior();
        public IMechanicalUnit CreateMechanical() => new BattleScorpion();
    }

public class ProtectedSkillsList
    {
        private readonly List<string> _internalList;
        private bool _isReadOnly;

        public ProtectedSkillsList(List<string> list, bool isReadOnly)
        {
            _internalList = list;
            _isReadOnly = isReadOnly;
        }

        public void Add(string item)
        {
            if (_isReadOnly)
                throw new InvalidOperationException("Ta jednostka jest kopią płytką.");

            _internalList.Add(item);
        }

        public IReadOnlyList<string> AsReadOnly() => _internalList.AsReadOnly();

        
        public ProtectedSkillsList DeepClone() => new ProtectedSkillsList(new List<string>(), false);

        
        public ProtectedSkillsList ShallowClone() => new ProtectedSkillsList(_internalList, true);
    }

    public class Hero : IGameUnit
    {
        public string Name {get; private set; }
        public string Helmet {get; private set; }
        public string Armor {get; private set; }
        public string Weapon{ get; private set; }

        private ProtectedSkillsList _protectedSkills;
        public IReadOnlyList<string> Skills => _protectedSkills.AsReadOnly();

        internal Hero(string name, string helmet, string armor, string weapon, List<string> skills)
        {
            Name = name;
            Helmet = helmet;
            Armor = armor;
            Weapon = weapon;
            _protectedSkills = new ProtectedSkillsList(skills, false);
        } 
        public string GetName() => Name;

        public void ChangeNameForCloneTest(string newName) => Name = newName;

        public void LearnSkill(string skill) => _protectedSkills.Add(skill);

        public IGameUnit Clone() 
        {
            return DeepCopy();
        }

        public Hero DeepCopy()
        {
            Hero clone = (Hero)this.MemberwiseClone();

            clone._protectedSkills = this._protectedSkills.DeepClone();

            return clone;
        }

        public Hero ShallowCopy()
        {
            Hero clone = (Hero)this.MemberwiseClone();

            clone._protectedSkills = this._protectedSkills.ShallowClone();

            return clone;
        }
    }

    public interface IHeroBuilder
    {
        IHeroBuilder SetName(string name);
        IHeroBuilder SetHelmet(string helmet);
        IHeroBuilder SetArmor(string armor);
        IHeroBuilder SetWeapon(string weapon);
        IHeroBuilder AddSkill(string skill);
        Hero Build();
    }

    public class HeroBuilder : IHeroBuilder 
    {
        private string _name;
        private string _helmet;
        private string _armor;
        private string _weapon;
        private List<string> _skills = new List<string>();

        public IHeroBuilder SetName(string name) { _name = name; return this; }
        public IHeroBuilder SetHelmet(string helmet) {_helmet = helmet; return this;}
        public IHeroBuilder SetArmor(string armor) {_armor = armor; return this;}
        public IHeroBuilder SetWeapon(string weapon) {_weapon = weapon; return this;}
        public IHeroBuilder AddSkill(string skill) { _skills.Add(skill); return this;}

        public Hero Build()
        {
            return new Hero(_name, _helmet, _armor, _weapon, new List<string>(_skills));
        }
    }

class Program
    {
        static void Main ()
        {
            var settings = GameEngineSettings.GetInstance();
            Console.WriteLine($"Konfiguracja załadowana. Max jednostek: {settings.MaxUnits}");

            IUnitFactory factory = new NorthernKingdomFactory();
            IInfantaryUnit infantary = factory.CreateInfantary();
            IMechanicalUnit mechanical = factory.CreateMechanical();

            Console.WriteLine($"\nWyprodukowano: {infantary.GetName()} oraz {mechanical.GetName()}");

            IHeroBuilder builder = new HeroBuilder();
            Hero aragorn = builder
                .SetName("Aragorn")
                .SetWeapon("Andruil")
                .SetArmor("Kolczuga")
                .AddSkill("Berserk")
                .AddSkill("Rzut Krasnoludem")
                .Build();

            Console.WriteLine("$\nStworzono bohatera!");

            Hero clonedHero = (Hero)aragorn.DeepCopy();
            Console.WriteLine("\nKlonujemy bohatera i dodajemy skille");

            clonedHero.ChangeNameForCloneTest("Boromir");
            clonedHero.LearnSkill("Górna Muka");
            clonedHero.LearnSkill("Syn Namiestnika");

            Hero clonedHero2 = (Hero)aragorn.ShallowCopy();
            clonedHero2.ChangeNameForCloneTest("Aragorn2");

            Console.WriteLine("\nPo klonowaniu nadane nowe imie");
            Console.WriteLine($"Oryginał: {aragorn.GetName()}, Skille oryginału: {string.Join(",",aragorn.Skills)}");
            Console.WriteLine($"Deep Klon: {clonedHero.GetName()}, Skille klona: {string.Join(", ", clonedHero.Skills)}");
            Console.WriteLine($"Shallow Klon: {clonedHero2.GetName()}, Skille klona: {string.Join(", ", clonedHero2.Skills)}");

        }
    }
}
