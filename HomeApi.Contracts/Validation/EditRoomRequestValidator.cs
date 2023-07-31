using FluentValidation;
using HomeApi.Contracts.Models.Rooms;
using System.Linq;

namespace HomeApi.Contracts.Validation
{
    public class EditRoomRequestValidator : AbstractValidator<EditRoomRequest>
    {
        public EditRoomRequestValidator()
        {
            RuleFor(room => room.NewArea).Must(CheckInt).WithMessage($"Площадь комнаты должна быть больше ноля.");
            RuleFor(room => room.NewVoltage).Must(CheckInt).InclusiveBetween(120, 220);
            RuleFor(room => room.ChangeGasConnection).InclusiveBetween(false, true);
            RuleFor(room => room.KeepDevices).InclusiveBetween(false, true);
            RuleFor(room => room.ChangeAddDate).InclusiveBetween(false, true);
            RuleFor(room => room.NewName).Must(BeSupported)
                .WithMessage($"Пожалуйста, выберете одну из следующих комнат: {string.Join(", ", Values.ValidRooms)}");
        }

        /// <summary>
        ///  Метод кастомной валидации для свойства location
        /// </summary>
        
        bool BeSupported(string location)
        {
            if (!string.IsNullOrEmpty(location))
                return Values.ValidRooms.Any(room => room == location);
            else
                return true;
        }

        /// <summary>
        /// Проверка ввдённого значения int?
        /// </summary>
        bool CheckInt(int? value)
        {
            return value.GreaterThanZeroOrIsNull();
        }
    }
}