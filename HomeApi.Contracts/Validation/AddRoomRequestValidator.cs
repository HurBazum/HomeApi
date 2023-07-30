using FluentValidation;
using HomeApi.Contracts.Models.Rooms;
using System.Linq;

namespace HomeApi.Contracts.Validation
{
    /// <summary>
    /// Класс-валидатор запросов на добавление новой комнаты
    /// </summary>
    public class AddRoomRequestValidator : AbstractValidator<AddRoomRequest>
    {
        public AddRoomRequestValidator() 
        {
            RuleFor(x => x.Area).NotEmpty(); 
            RuleFor(x => x.Name).NotEmpty()
                .Must(BeSupported).WithMessage($"Пожалуйста, выберете одну из следующих комнат: {string.Join(", ", Values.ValidRooms)}");
            RuleFor(x => x.Voltage).NotEmpty().GreaterThan(120);
            RuleFor(x => x.GasConnected).InclusiveBetween(false, true);
        }

        bool BeSupported(string location)
        {
            return Values.ValidRooms.Any(room => room == location);
        }
    }
}