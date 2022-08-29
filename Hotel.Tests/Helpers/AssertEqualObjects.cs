using Hotel.Server.Helpers;
using Xunit;

namespace Hotel.Tests;

public class AssertEqualObjects
{
    public static void Check (object source, object target)
    {
        var comparer = new JsonElementComparer();
        var sourceJson = System.Text.Json.JsonDocument.Parse(Json.Serialize(source));
        var targetJson = System.Text.Json.JsonDocument.Parse(Json.Serialize(target));
        Assert.True(comparer.Equals(sourceJson.RootElement, targetJson.RootElement));
    }
}


