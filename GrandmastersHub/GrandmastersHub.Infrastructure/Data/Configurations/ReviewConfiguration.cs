using GrandmastersHub.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.Metadata.Builders;



namespace GrandmastersHub.Infrastructure.Data.Configurations;



public class ReviewConfiguration : IEntityTypeConfiguration<Review>

{

    public void Configure(EntityTypeBuilder<Review> builder)

    {

        builder.HasKey(r => r.ReviewId);



        builder.Property(r => r.Rating)

            .IsRequired();



        builder.Property(r => r.Comment)

            .HasMaxLength(1000);



        builder.HasIndex(r => new { r.UserId, r.ProductId })

            .IsUnique();

    }

}