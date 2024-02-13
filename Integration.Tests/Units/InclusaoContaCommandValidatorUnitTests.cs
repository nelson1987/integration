using FluentValidation.TestHelper;

namespace Integration.Tests.Units;

public class InclusaoContaCommandValidatorUnitTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly IValidator<InclusaoContaCommand> _validator;
    private readonly InclusaoContaCommand _command;

    public InclusaoContaCommandValidatorUnitTests()
    {
        _command = _fixture.Build<InclusaoContaCommand>()
            .Create();
        _validator = _fixture.Create<InclusaoContaCommandValidator>();
    }

    [Fact]
    public void Given_a_valid_event_when_all_fields_are_valid_should_pass_validation()
        => _validator
            .TestValidate(_command)
            .ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Given_a_cancellation_request_with_invalid_asset_id_should_fail_validation()
        => _validator
            .TestValidate(_command with { NomeTitular = string.Empty })
            .ShouldHaveValidationErrorFor(x => x.NomeTitular)
            .Only();
}
