using FluentValidation;
using HomeApi.Contracts.Models.Rooms;

namespace HomeApi.Contracts.Validation
{
    public class EditRoomRequestValidator : AbstractValidator<EditRoomRequest>
    {
        public EditRoomRequestValidator() 
        {
            RuleFor(room => room.NewArea).NotEmpty();
            RuleFor(room => room.NewVoltage).NotEmpty().InclusiveBetween(120, 220);
            RuleFor(room => room.ChangeGasConnection).InclusiveBetween(false, true);
            RuleFor(room => room.KeepDevices).InclusiveBetween(false, true);
        }
    }
}