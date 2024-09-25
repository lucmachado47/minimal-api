using minimal_api.Domain.Entities;
using minimal_api.Migrations;

namespace Test.Domain.Entities;

[TestClass]
public class VehicleTest
{
    [TestMethod]
    public void TestarGetSetPropriedades()
    {
        var vehicle = new Vehicle();

        vehicle.Id = 1;
        vehicle.Nome = "Jetta";
        vehicle.Marca = "Volkswaggen";
        vehicle.Ano = 2024;

        Assert.AreEqual(1, vehicle.Id);
        Assert.AreEqual("Jetta", vehicle.Nome);
        Assert.AreEqual("Volkswaggen", vehicle.Marca);
        Assert.AreEqual(2024, vehicle.Ano);
    }
}