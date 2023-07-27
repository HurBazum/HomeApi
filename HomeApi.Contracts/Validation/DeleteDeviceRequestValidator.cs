using FluentValidation;
using HomeApi.Contracts.Models.Devices;
using System.Linq;

namespace HomeApi.Contracts.Validation
{
    /// <summary>
    /// Класс-валидатор запросов на удаление девайсов
    /// </summary>
    public class DeleteDeviceRequestValidator : AbstractValidator<DeleteDeviceRequest>
    {
        public DeleteDeviceRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.RoomLocation)
                .NotEmpty()
                .Must(BeSupported)
                .WithMessage($"Пожалуйста, выберете одну из следующих комнат: {string.Join(", ", Values.ValidRooms)}");
        }

        bool BeSupported(string location)
        {
            return Values.ValidRooms.Any(room => room == location);
        }
    }
}