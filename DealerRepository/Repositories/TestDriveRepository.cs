using AgencyRepository.Data;
using AgencyRepository.Model;
using Microsoft.EntityFrameworkCore;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                .Where(td => td.AppointmentDate == date)
                .ToListAsync();
        }

        public async Task<IEnumerable<TestDrive>> GetTestDrivesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.TestDrives
                .Include (td => td.Agency)
                .Where(td => td.AppointmentDate >= startDate && td.AppointmentDate <= endDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<TestDrive>> GetTestDrivesByAgencyIdAsync(int AgencyId)
        {
            return await _context.TestDrives
                .Include(d => d.Agency)
                .Where(td => td.AgencyId == AgencyId)
                .ToListAsync();
        }

        public Task<IEnumerable<TestDrive>> GetTestDrivesByStatusAsync(string status)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TestDrive>> GetTestDrivesByVehicleIdAsync(int vehicleId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsScheduleConflictAsync(int vehicleId, DateTime appointmentDate)
        {
            throw new NotImplementedException();
        }
    }
}
