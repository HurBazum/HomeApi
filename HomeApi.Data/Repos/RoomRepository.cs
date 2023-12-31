﻿using System;
using System.Linq;
using System.Threading.Tasks;
using HomeApi.Data.Models;
using HomeApi.Data.Queries;
using Microsoft.EntityFrameworkCore;

namespace HomeApi.Data.Repos
{
    /// <summary>
    /// Репозиторий для операций с объектами типа "Room" в базе
    /// </summary>
    public class RoomRepository : IRoomRepository
    {
        private readonly HomeApiContext _context;
        
        public RoomRepository (HomeApiContext context)
        {
            _context = context;
        }
        
        /// <summary>
        ///  Найти комнату по имени
        /// </summary>
        public async Task<Room> GetRoomByName(string name)
        {
            return await _context.Rooms.Where(r => r.Name == name).FirstOrDefaultAsync();
        }
        
        /// <summary>
        ///  Добавить новую комнату
        /// </summary>
        public async Task AddRoom(Room room)
        {
            var entry = _context.Entry(room);
            if (entry.State == EntityState.Detached)
                await _context.Rooms.AddAsync(room);
            
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Получить все комнаты
        /// </summary>
        public async Task<Room[]> GetAll()
        {
            return await _context.Rooms.AsNoTracking().ToArrayAsync();
        }

        /// <summary>
        /// Обновить данные комнаты
        /// </summary>
        public async Task Update(Room room, UpdateRoomQuery updateRoomQuery)
        {
            if(updateRoomQuery.NewArea != null) room.Area = Convert.ToInt32(updateRoomQuery.NewArea);
            if(updateRoomQuery.ChangeGasConnection != default) room.GasConnected = !room.GasConnected;
            if(updateRoomQuery.NewVoltage != null) room.Voltage = Convert.ToInt32(updateRoomQuery.NewVoltage);
            if(updateRoomQuery.UpdateDate != null) room.AddDate = Convert.ToDateTime(updateRoomQuery.UpdateDate);
            if(!string.IsNullOrEmpty(updateRoomQuery.NewName)) room.Name = updateRoomQuery.NewName;

            var entry = _context.Entry(room);
            if(entry.State == EntityState.Detached)
                _context.Rooms.Update(room);

            await _context.SaveChangesAsync();
        }

        public async Task Delete(Room room)
        {
            var entry = _context.Entry(room);
            if (entry.State == EntityState.Detached)
                _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
        }
    }
}