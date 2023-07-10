using System.Runtime.CompilerServices;
using System.Text.Json;
using Jose;

namespace JoseJwtReproduction;

public class Program
{
    public static void Main(string[] args)
    {
        var sourceJwtModelNoDecimals = new SourceJwtModel
        {
            jti = "jti1",
            sub = "sub1",
            iat = 1688784499L,

            addOn = new SourceJwtAddOnModel(
                name: "500 widgets/year",
                amount: 24m // <-- No decimal places
                ),
        };

        var sourceJwtModelTwoDecimals = new SourceJwtModel
        {
            jti = "jti2",
            sub = "sub2",
            iat = 1688784499L,

            addOn = new SourceJwtAddOnModel(
                name: "500 widgets/year",
                amount: 24.01m // <-- Two decimal places
                ),
        };


        //
        // System.Text.Json: default, out of the box Encode and Decode<T>.
        //

        Console.WriteLine("***");
        Console.WriteLine("*** Encode, then Decode<T>");
        Console.WriteLine("***");
        Console.WriteLine();

        EncodeAndDecodeJWTToDestinationType<DestinationJwtModel>(sourceJwtModelNoDecimals);
        EncodeAndDecodeJWTToDestinationType<DestinationIDictionaryJwtModel>(sourceJwtModelNoDecimals);

        EncodeAndDecodeJWTToDestinationType<DestinationJwtModel>(sourceJwtModelTwoDecimals);
        EncodeAndDecodeJWTToDestinationType<DestinationIDictionaryJwtModel>(sourceJwtModelTwoDecimals);


        //
        // System.Text.Json: Encode, Decode to string, JsonSerializer.Deserialize
        //

        Console.WriteLine();
        Console.WriteLine("***");
        Console.WriteLine("*** Encode, Decode to string, then JsonSerializer.Deserialize<T>");
        Console.WriteLine("***");
        Console.WriteLine();

        EncodeAndDecodeJWTToStringThenDestinationType<DestinationJwtModel>(sourceJwtModelNoDecimals);
        EncodeAndDecodeJWTToStringThenDestinationType<DestinationIDictionaryJwtModel>(sourceJwtModelNoDecimals);

        EncodeAndDecodeJWTToStringThenDestinationType<DestinationJwtModel>(sourceJwtModelTwoDecimals);
        EncodeAndDecodeJWTToStringThenDestinationType<DestinationIDictionaryJwtModel>(sourceJwtModelTwoDecimals);
    }


    private static readonly byte[] _keyBytes = Convert.FromBase64String("7dS4cd7dPmRK3fuYpcrfa7UNJFsPmC3atnxGf3+DkaQ=");

    private static readonly JsonSerializerOptions _serializeOptions = new()
    {
        WriteIndented = true,
    };

    private static void EncodeAndDecodeJWTToDestinationType<TDestination>(SourceJwtModel sourceJwtModel, [CallerArgumentExpression(nameof(sourceJwtModel))] string? callerArgExpression = null)
    {
        var serializerName = JWT.DefaultSettings.JsonMapper.GetType().FullName;

        // Encode the source JWT model
        var jwt = JWT.Encode(sourceJwtModel, _keyBytes, JwsAlgorithm.HS256);

        try
        {
            // Decode the JWT into a separate, destination model.
            var destinationJWTModel = JWT.Decode<TDestination>(jwt, _keyBytes, JwsAlgorithm.HS256);

            Console.WriteLine($"Successfully encoded and decoded {callerArgExpression} with jti={sourceJwtModel.jti} for type {typeof(TDestination).FullName} using {serializerName}.");
            Console.WriteLine($"JWT: {jwt}");
            Console.WriteLine($"Decoded model JSON with System.Text.Json: {JsonSerializer.Serialize(destinationJWTModel, _serializeOptions)}");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"!!! Failed to encode and decode {callerArgExpression} with jti={sourceJwtModel.jti} for type {typeof(TDestination).FullName} using {serializerName} !!!");
            Console.WriteLine($"JWT: {jwt}");
            Console.WriteLine(ex.Message);
        }
        finally
        {
            Console.WriteLine();
        }
    }


    private static readonly JsonSerializerOptions _deserializeOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static void EncodeAndDecodeJWTToStringThenDestinationType<TDestination>(SourceJwtModel sourceJwtModel, [CallerArgumentExpression(nameof(sourceJwtModel))] string? callerArgExpression = null)
    {
        var serializerName = JWT.DefaultSettings.JsonMapper.GetType().FullName;

        // Encode the source JWT model
        var jwt = JWT.Encode(sourceJwtModel, _keyBytes, JwsAlgorithm.HS256);

        try
        {
            // Decode the JWT into a string first.
            var decodedJWTString = JWT.Decode(jwt, _keyBytes, JwsAlgorithm.HS256);

            // Deserialize the decoded JWT into a separate, destination model.
            var destinationJWTModel = JsonSerializer.Deserialize<TDestination>(decodedJWTString, _deserializeOptions);

            Console.WriteLine($"Successfully encoded, decoded to string, and deserialized {callerArgExpression} with jti={sourceJwtModel.jti} for type {typeof(TDestination).FullName} using {serializerName}.");
            Console.WriteLine($"JWT: {jwt}");

            Console.WriteLine($"Decoded model JSON with System.Text.Json: {JsonSerializer.Serialize(destinationJWTModel, _serializeOptions)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"!!! Failed to encode, decode to string, and deserialize {callerArgExpression} with jti={sourceJwtModel.jti} for type {typeof(TDestination).FullName} using {serializerName} !!!");
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

/// <summary>
/// The destination payload model. Think of the receiver of the JWT as residing in a totally different system.
/// </summary>
/// <remarks>
/// Instead of using object as addOn's type, it uses an IDictionary&lt;string, object&gt;.
/// </remarks>
public class DestinationIDictionaryJwtModel
{
    public string jti { get; set; } = null!;
    public string sub { get; set; } = null!;
    public long iat { get; set; }
    public IDictionary<string, object> addOn { get; set; } = null!;
}