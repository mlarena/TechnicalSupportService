using Microsoft.EntityFrameworkCore;
using TechnicalSupportService.Core.DTOs;
using TechnicalSupportService.Core.Enums;
using TechnicalSupportService.Core.Exceptions;
using TechnicalSupportService.Core.Interfaces;
using TechnicalSupportService.Data.Context;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.SUTP.Services;

public class CommentService : ICommentService
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditService _audit;

    public CommentService(ApplicationDbContext db, IAuditService audit)
    { _db = db; _audit = audit; }

    public async Task<List<CommentDto>> GetByTicketAsync(Guid ticketId, string currentRole)
    {
        var query = _db.Comments.Include(c => c.AuthorUser)
            .Where(c => c.TicketId == ticketId && !c.IsDeleted);
        if (currentRole == Core.Constants.Roles.Applicant)
            query = query.Where(c => !c.IsInternal);

        return await query.OrderByDescending(c => c.CreatedAt)
            .Select(c => new CommentDto
            {
                Id = c.Id, AuthorName = c.AuthorUser.FullName, Content = c.Content,
                IsInternal = c.IsInternal, IsEdited = c.IsEdited, CreatedAt = c.CreatedAt
            }).ToListAsync();
    }

    public async Task<CommentDto> AddAsync(Guid ticketId, CommentCreateDto dto, Guid currentUserId)
    {
        var comment = new Comment
        {
            TicketId = ticketId, AuthorUserId = currentUserId,
            Content = dto.Content, IsInternal = dto.IsInternal, CreatedAt = DateTime.UtcNow
        };
        _db.Comments.Add(comment);
        _db.TicketHistories.Add(new TicketHistory
        {
            TicketId = ticketId, ChangedByUserId = currentUserId, ChangeType = ChangeType.Comment,
            CommentId = comment.Id, NewValue = dto.Content.Length > 100 ? dto.Content[..100] + "..." : dto.Content
        });
        await _audit.LogAsync("Comment.Add", currentUserId, "Comment", comment.Id);
        await _db.SaveChangesAsync();

        var author = await _db.Users.FindAsync(currentUserId);
        return new CommentDto
        {
            Id = comment.Id, AuthorName = author?.FullName ?? "", Content = comment.Content,
            IsInternal = comment.IsInternal, IsEdited = false, CreatedAt = comment.CreatedAt
        };
    }

    public async Task<CommentDto> EditAsync(Guid commentId, string newContent, Guid currentUserId)
    {
        var comment = await _db.Comments.Include(c => c.AuthorUser).FirstOrDefaultAsync(c => c.Id == commentId)
            ?? throw new NotFoundException("Комментарий не найден");
        if (comment.AuthorUserId != currentUserId) throw new ForbiddenException("Можно редактировать только свои комментарии");
        comment.Content = newContent; comment.IsEdited = true; comment.EditedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return new CommentDto
        {
            Id = comment.Id, AuthorName = comment.AuthorUser.FullName, Content = comment.Content,
            IsInternal = comment.IsInternal, IsEdited = true, CreatedAt = comment.CreatedAt
        };
    }

    public async Task DeleteAsync(Guid commentId, Guid currentUserId)
    {
        var comment = await _db.Comments.FindAsync(commentId) ?? throw new NotFoundException("Комментарий не найден");
        comment.IsDeleted = true;
        await _db.SaveChangesAsync();
    }
}
