using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace StrategyGame.Tests
{
    [TestClass]
    public class FullDesignPatternTests
    {
        // --- 1. TESTY SINGLETONA (Zadanie 1 - 25%) ---

        [TestMethod]
        public void Singleton_ThreadSafety_ShouldReturnSameInstance()
        {
           // Sprawdza bezpieczeństwo wątkowe (Wymóg: 10%)
           GameEngineSettings instance1 = null;
           GameEngineSettings instance2 = null;

           Parallel.Invoke(
               () => instance1 = GameEngineSettings.GetInstance(),
               () => instance2 = GameEngineSettings.GetInstance()
           );

           Assert.AreSame(instance1, instance2, "Singleton nie jest bezpieczny wątkowo!");
        }

        [TestMethod]
        public void Singleton_ReflectionResistance_ShouldThrow()
        {
            var instance = GameEngineSettings.GetInstance();

            // Sprawdza odporność na refleksję (Wymóg: 5%)
            ConstructorInfo ctor = typeof(GameEngineSettings).GetConstructor(
               BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null);

           Assert.ThrowsException<TargetInvocationException>(() => ctor.Invoke(null));
        }

        // --- 2. TESTY FABRYKI ABSTRAKCYJNEJ (Zadanie 2 - 30%) ---

        [TestMethod]
        public void Factory_Compatibility_TeamUpShouldWork()
        {
           // Sprawdza kompatybilność produktów z tej samej fabryki (Wymóg: 5%)
           IUnitFactory factory = new NorthernKingdomFactory();
           var knight = factory.CreateInfantary();
           var archer = factory.CreateMechanical();

           string result = archer.TeamUp(knight);

           Assert.IsTrue(result.Contains("Rycerz") && result.Contains("Łucznik"),
               "Jednostki z tej samej fabryki powinny móc ze sobą współpracować.");
        }

        // --- 3. TESTY BUILDERA (Zadanie 3 - 25%) ---

        [TestMethod]
        public void Builder_FluentInterface_ShouldBeChainable()
        {
           // Sprawdza czy builder jest typu Fluent (Wymóg: 10%)
           var builder = new HeroBuilder();
           var result = builder.SetName("A").SetWeapon("B").AddSkill("C");

           Assert.IsInstanceOfType(result, typeof(IHeroBuilder), "Metody powinny zwracać instancję buildera.");
        }

        [TestMethod]
        public void Hero_Encapsulation_NoPublicSetters()
        {
           // Sprawdza hermetyzację produktu (Wymóg: kara -5% za publiczne settery)
           PropertyInfo nameProp = typeof(Hero).GetProperty("Name");
           Assert.IsFalse(nameProp.CanWrite && nameProp.GetSetMethod(true).IsPublic,
               "Bohater nie powinien mieć publicznych setterów!");
        }

        // --- 4. TESTY PROTOTYPU (Zadanie 4 - 20%) ---

        [TestMethod]
        public void Hero_ShallowCopy_SharesReferenceTypes()
        {
           // Sprawdza kopię płytką (Wymóg: 5%)
           var builder = new HeroBuilder();
           Hero h1 = builder.SetName("Oryginał").AddSkill("Ogień").Build();
           Hero h2 = h1.ShallowCopy();

            h1.LearnSkill("Lód");
            h1.LearnSkill("Błyskawica");

            // W kopii płytkiej lista jest współdzielona
            Assert.AreEqual(h1.Skills.Count, h2.Skills.Count, "Płytka kopia powinna współdzielić referencje.");
            Assert.ThrowsException<InvalidOperationException>(() => h2.LearnSkill("Błyskawica"), 
                "Zwykły klon nie może sam dodawać skilli.");  
        }

        [TestMethod]
        public void Hero_DeepCopy_IndependentReferenceTypes()
        {
           // Sprawdza kopię głęboką (Wymóg: 10%)
           var builder = new HeroBuilder();
           Hero h1 = builder.SetName("Oryginał").AddSkill("Ogień").Build();
           Hero h2 = h1.DeepCopy();

            h2.LearnSkill("Lód");
            h2.LearnSkill("Błyskawica");

            // W kopii głębokiej listy muszą być niezależne
            Assert.AreNotEqual(h1.Skills.Count, h2.Skills.Count, "Głęboka kopia musi tworzyć nową instancję listy.");
        }
    }
}