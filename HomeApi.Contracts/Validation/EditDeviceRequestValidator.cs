using System.Linq;
using FluentValidation;
using HomeApi.Contracts.Models.Devices;

namespace HomeApi.Contracts.Validation
{
    /// <summary>
    /// Класс-валидатор запросов обновления устройства
    /// </summary>
    public class EditDeviceRequestValidator : AbstractValidator<EditDeviceRequest>
    {
        /// <summary>
        /// Метод, конструктор, устанавливающий правила
        /// </summary>
        public EditDeviceRequestValidator() 
        {
            RuleFor(x => x.NewName).NotEmpty(); 
            RuleFor(x => x.NewRoom).NotEmpty().Must(BeSupported)
                .WithMessage($"Пожалуйста, выберете одну из следующих комнат: {string.Join(", ", Values.ValidRooms)}");
        }
        
        /// <summary>
        ///  Метод кастомной валидации для свойства location
        /// </summary>
        bool BeSupported(string location)
        {
            return Values.ValidRooms.Any(room => room == location);
        }
    }
}