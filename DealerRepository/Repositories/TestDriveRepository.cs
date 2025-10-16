using AgencyRepository.Data;
using AgencyRepository.Model;
using Microsoft.EntityFrameworkCore;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgencyRepository.Repositories
{
    public class TestDriveRepository : GenericRepository<TestDrive>, ITestDriveRepository
    {
        private readonly AgencyDbContext _context;
        public TestDriveRepository(AgencyDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<TestDrive?> GetDetailByIdAsync(int id)
        {
            return await _context.TestDrives
                .Include(td => td.Agency)
                .FirstOrDefaultAsync(td => td.Id == id);
        }

        public async Task<IEnumerable<TestDrive>> GetTestDrivesByDateAsync(DateTime date)
        {
            return await _context.TestDrives
                .Include(td => td.Agency)
                .Where(td => td.AppointmentDate.HasValue && td.AppointmentDate.Value.Date == date.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<TestDrive>> GetTestDrivesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.TestDrives
                .Include(td => td.Agency)
                .Where(td => td.AppointmentDate.HasValue && td.AppointmentDate.Value.Date >= startDate.Date && td.AppointmentDate.Value.Date <= endDate.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<TestDrive>> GetTestDrivesByAgencyIdAsync(int agencyId)
        {
            return await _context.TestDrives
                .Include(d => d.Agency)
                .Where(td => td.AgencyId == agencyId)
                .ToListAsync();
        }

        public async Task<IEnumerable<TestDrive>> GetTestDrivesByStatusAsync(string status)
        {
            return await _context.TestDrives
                .Include(td => td.Agency)
                .Where(td => td.Status != null && td.Status.ToLower() == status.ToLower())
                .ToListAsync();
        }

        public async Task<IEnumerable<TestDrive>> GetTestDrivesByVehicleIdAsync(int vehicleInstanceId)
        {
            return await _context.TestDrives
                .Include(td => td.Agency)
                .Where(td => td.VehicleInstanceId == vehicleInstanceId)
                .ToListAsync();
        }
    }
}

