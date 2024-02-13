namespace Integration.Tests.Units;

public class AutoMapperProfileUnitTests
{
    [Fact]
    public void ValidateMappingConfigurationTest()
    {
        var mapper = ObjectMapper.Mapper;

        mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }
}