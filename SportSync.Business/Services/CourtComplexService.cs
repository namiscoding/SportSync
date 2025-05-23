﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SportSync.Business.Interfaces;
using SportSync.Business.Dtos; // **THÊM USING CHO DTOS**
using SportSync.Data;
using SportSync.Data.Entities;
using SportSync.Data.Enums;
// using SportSync.Web.Models.ViewModels.CourtComplex; // **XÓA USING NÀY**
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportSync.Business.Services
{
    public class CourtComplexService : ICourtComplexService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IImageUploadService _imageUploadService;
        private readonly ILogger<CourtComplexService> _logger;

        public CourtComplexService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IImageUploadService imageUploadService,
            ILogger<CourtComplexService> logger)
        {
            _context = context;
            _userManager = userManager;
            _imageUploadService = imageUploadService;
            _logger = logger;
        }

        public async Task<IEnumerable<CourtComplex>> GetCourtComplexesByOwnerAsync(string ownerUserId)
        {
            // ... (giữ nguyên logic) ...
            if (string.IsNullOrEmpty(ownerUserId))
            {
                _logger.LogWarning("GetCourtComplexesByOwnerAsync: ownerUserId is null or empty.");
                return new List<CourtComplex>();
            }

            try
            {
                return await _context.CourtComplexes
                                     .Where(cc => cc.OwnerUserId == ownerUserId)
                                     .OrderByDescending(cc => cc.CreatedAt)
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching court complexes for owner {OwnerUserId}", ownerUserId);
                return new List<CourtComplex>();
            }
        }

        // **THAY ĐỔI VIEWMODEL THÀNH DTO**
        public async Task<(bool Success, CourtComplex CreatedComplex, IEnumerable<string> Errors)> CreateCourtComplexAsync(CreateCourtComplexDto dto, string ownerUserId)
        {
            var errors = new List<string>();
            if (dto == null)
            {
                errors.Add("Dữ liệu khu phức hợp không được để trống.");
                return (false, null, errors);
            }
            if (string.IsNullOrEmpty(ownerUserId))
            {
                errors.Add("Không xác định được người sở hữu.");
                return (false, null, errors);
            }

            // Mapping từ DTO sang Entity
            var courtComplex = new CourtComplex
            {
                OwnerUserId = ownerUserId,
                Name = dto.Name,
                Address = dto.Address,
                City = dto.City,
                District = dto.District,
                Ward = dto.Ward,
                Description = dto.Description,
                ContactPhoneNumber = dto.ContactPhoneNumber,
                ContactEmail = dto.ContactEmail,
                DefaultOpeningTime = dto.DefaultOpeningTime,
                DefaultClosingTime = dto.DefaultClosingTime,
                CreatedAt = DateTime.UtcNow,
                ApprovalStatus = ApprovalStatus.PendingApproval,
                IsActiveByOwner = true,
                IsActiveByAdmin = false
            };

            // Xử lý tải ảnh từ DTO
            if (dto.MainImage != null && dto.MainImage.Content != null && dto.MainImage.Content.Length > 0)
            {
                _logger.LogInformation("Attempting to upload main image for new court complex: {FileName}", dto.MainImage.FileName);
                string folderName = $"court_complexes/{Guid.NewGuid()}";

                // Gọi service upload ảnh với ImageInputDto
                var uploadResult = await _imageUploadService.UploadImageAsync(dto.MainImage, folderName);

                if (uploadResult.Success)
                {
                    courtComplex.MainImageCloudinaryPublicId = uploadResult.PublicId;
                    courtComplex.MainImageCloudinaryUrl = uploadResult.Url;
                    _logger.LogInformation("Main image uploaded successfully: {ImageUrl}", uploadResult.Url);
                }
                else
                {
                    _logger.LogWarning("Main image upload failed: {ErrorMessage}", uploadResult.ErrorMessage);
                    errors.Add("Lỗi tải ảnh đại diện: " + (uploadResult.ErrorMessage ?? "Không xác định."));
                }
            }

            // Nếu có lỗi từ việc tải ảnh và bạn muốn dừng lại, hãy kiểm tra errors ở đây
            if (errors.Any(e => e.StartsWith("Lỗi tải ảnh đại diện")))
            {
                // return (false, null, errors); // Bỏ comment nếu muốn dừng khi tải ảnh lỗi
            }


            try
            {
                _context.CourtComplexes.Add(courtComplex);
                await _context.SaveChangesAsync();
                _logger.LogInformation("CourtComplex '{ComplexName}' created successfully by User {OwnerUserId}. ID: {ComplexId}", courtComplex.Name, ownerUserId, courtComplex.CourtComplexId);
                return (true, courtComplex, null); // Trả về null cho errors nếu thành công
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DbUpdateException while creating CourtComplex '{ComplexName}' by User {OwnerUserId}.", dto.Name, ownerUserId);
                errors.Add("Đã xảy ra lỗi khi lưu thông tin vào cơ sở dữ liệu. Vui lòng thử lại.");
                return (false, null, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic exception while creating CourtComplex '{ComplexName}' by User {OwnerUserId}.", dto.Name, ownerUserId);
                errors.Add("Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.");
                return (false, null, errors);
            }
        }
    }
}
