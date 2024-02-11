using minimal_api.dominio.entidades;

namespace Test.Domain.Entidades;

[TestClass]

public class VeiculosTest
{
    [TestMethod]
    public void TestarGetSetPropriedades()
    {
        //Arrange
        var adm = new Veiculo
        {
            //Act
            Id = 1,
            Nome = "Fusca",
            Marca = "VolksWagem",
            Ano = 1998
        };

        //Assert
        Assert.AreEqual(1, adm.Id);
        Assert.AreEqual("Fusca", adm.Nome);
        Assert.AreEqual("VolksWagem", adm.Marca);
        Assert.AreEqual(1998, adm.Ano);

        //dotnet test

    }
}
