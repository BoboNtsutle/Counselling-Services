using Ancora.Web.Models;
using Ancora.Web.Services.Supabase;

namespace Ancora.Web.Services;

/// <summary>
/// MODULE 4 — Admin: Announcements, programmes &amp; events (FR-08, FR-12)
/// Whatever is saved here shows up in the students' Flutter app straight away.
/// Owner: see TEAM-PLAN.md
/// </summary>
public class AnnouncementService
{
    public const string ImageBucket = "announcement-images";
    public const long MaxImageBytes = 8 * 1024 * 1024; // same 8 MB limit as the bucket
    public static readonly string[] AllowedImageTypes = { "image/jpeg", "image/png", "image/webp", "image/gif" };

    private readonly SupabaseClient _db;

    public AnnouncementService(SupabaseClient db) => _db = db;

    public Task<List<Announcement>> GetAllAsync() =>
        _db.GetAsync<Announcement>("announcements", "order=created_at.desc");

    // TODO (Module 4): publish an announcement, with an optional image.
    // Steps (mirror publishAnnouncement in the Flutter app's supabase_appointment_repository.dart):
    //  1. If image != null: check image.Length <= MaxImageBytes and image.ContentType is in AllowedImageTypes.
    //  2. Path MUST start with the admin's user id (the storage policy checks the owner):
    //       var path = $"{adminUserId}/{DateTime.UtcNow.Ticks}{Path.GetExtension(image.FileName)}";
    //       await using var stream = image.OpenReadStream();
    //       imageUrl = await _db.UploadAsync(ImageBucket, path, stream, image.ContentType);
    //  3. await _db.InsertAsync<Announcement>("announcements",
    //         new { title, message, image_url = imageUrl });
    public async Task PublishAsync(Guid adminUserId, string title, string message, IFormFile? image)
    {
        string? imageUrl = null;
        if (image != null && image.Length > 0)
        {
            if (image.Length > MaxImageBytes)
                throw new ArgumentException($"Image exceeds maximum size of {MaxImageBytes} bytes");
            if (!AllowedImageTypes.Contains(image.ContentType))
                throw new ArgumentException("Unsupported image type");

            var ext = Path.GetExtension(image.FileName);
            var path = $"{adminUserId}/{DateTime.UtcNow.Ticks}{ext}";
            await using var stream = image.OpenReadStream();
            imageUrl = await _db.UploadAsync(ImageBucket, path, stream, image.ContentType ?? "application/octet-stream");
        }

        await _db.InsertAsync<Announcement>("announcements", new { title, message, image_url = imageUrl });
    }

    // TODO (Module 4): delete → await _db.DeleteAsync("announcements", $"id=eq.{id}");
    public Task DeleteAsync(Guid id) =>
        _db.DeleteAsync("announcements", $"id=eq.{id}");
}
