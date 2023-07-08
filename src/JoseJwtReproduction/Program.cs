using Jose;

namespace JoseJwtReproduction;

public class Program
{
    public static void Main(string[] args)
    {
        // Encoding/Decoding succeeds when amount is a System.Decimal with no decimal places.
        var sourceJwtModel1 = new SourceJwtModel
        {
            jti = "jti1",
            sub = "sub1",
            iat = 1688784499L,

            addOn = new SourceJwtAddOnModel(
                name: "500 widgets/year",
                amount: 24m
                ),
        };

        EncodeAndDecodeJWT(sourceJwtModel1);

        // Encoding succeeds, but Decoding fails when amount is a System.Decimal with 1 or more decimal places.
        var sourceJwtModel2 = new SourceJwtModel
        {
            jti = "jti2",
            sub = "sub2",
            iat = 1688784499L,

            addOn = new SourceJwtAddOnModel(
                name: "500 widgets/year",
                amount: 24.00m
                ),
        };

        EncodeAndDecodeJWT(sourceJwtModel2);
    }

    private static readonly byte[] _keyBytes = Convert.FromBase64String("7dS4cd7dPmRK3fuYpcrfa7UNJFsPmC3atnxGf3+DkaQ=");

    private static void EncodeAndDecodeJWT(SourceJwtModel sourceJwtModel)
    {
        // Encode the source JWT model
        var jwt = JWT.Encode(sourceJwtModel, _keyBytes, JwsAlgorithm.HS256);

        try
        {
            // Decode the JWT into a separate, destination model.
            var destinationJwtModel = JWT.Decode<DestinationJwtModel>(jwt, _keyBytes, JwsAlgorithm.HS256);

            Console.WriteLine($"Successfully encoded and decoded jti={sourceJwtModel.jti}.");
            Console.WriteLine($"JWT: {jwt}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"!!! Failed to encode and decode jti={sourceJwtModel.jti} !!!");
            Console.WriteLine($"JWT: {jwt}");
            Console.WriteLine(ex.ToString());
        }
        finally
        {
            Console.WriteLine();
        }
    }
}


//
// Models
//

/// <summary>
/// The main JWT payload model.
/// </summary>
public class SourceJwtModel
{
    public string jti { get; set; } = null!;
    public string sub { get; set; } = null!;
    public long iat { get; set; }

    public SourceJwtAddOnModel addOn { get; set; } = null!;
}

/// <summary>
/// A nested property on the source JWT payload model.
/// </summary>
public record SourceJwtAddOnModel(
    string name,
    decimal amount
    );

/// <summary>
/// The destination payload model. Think of the receiver of the JWT as residing in a totally different system.
/// </summary>
public class DestinationJwtModel
{
    public string jti { get; set; } = null!;
    public string sub { get; set; } = null!;
    public long iat { get; set; }
    public object addOn { get; set; } = null!;
}