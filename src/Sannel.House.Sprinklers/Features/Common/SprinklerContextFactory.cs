using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Sannel.House.Sprinklers.Features.Common;

internal class SprinklerContextFactory : IDesignTimeDbContextFactory<SprinklerDbContext>
{
	public SprinklerDbContext CreateDbContext(string[] args)
	{
		var optionsBuilder = new DbContextOptionsBuilder<SprinklerDbContext>();
		optionsBuilder.UseSqlite("Data Source=blog.db");

		return new SprinklerDbContext(optionsBuilder.Options);
	}
}
