using minimal_api.Domain.Entities;
using minimal_api.Migrations;

namespace Test.Domain.Entities;

[TestClass]
public class AdmTest
{
    [TestMethod]
    public void TestarGetSetPropriedades()
    {
        var adm = new Adm();

        adm.Id = 1;
        adm.Email = "teste@teste.com";
        adm.Senha = "teste";
        adm.Perfil = "Adm";

        Assert.AreEqual(1, adm.Id);
        Assert.AreEqual("teste@teste.com", adm.Email);
        Assert.AreEqual("teste", adm.Senha);
        Assert.AreEqual("Adm", adm.Perfil);
    }
}