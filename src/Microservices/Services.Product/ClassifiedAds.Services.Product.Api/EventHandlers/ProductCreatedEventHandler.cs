﻿using ClassifiedAds.Application;
using ClassifiedAds.CrossCuttingConcerns.ExtensionMethods;
using ClassifiedAds.Domain.Events;
using ClassifiedAds.Infrastructure.Identity;
using ClassifiedAds.Services.AuditLog.Contracts.DTOs;
using ClassifiedAds.Services.Product.Api.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ClassifiedAds.Services.Product.EventHandlers
{
    public class ProductCreatedEventHandler : IDomainEventHandler<EntityCreatedEvent<Entities.Product>>
    {
        private readonly IServiceProvider _serviceProvider;

        public ProductCreatedEventHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Handle(EntityCreatedEvent<Entities.Product> domainEvent)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var currentUser = serviceProvider.GetService<ICurrentUser>();
                var dispatcher = serviceProvider.GetService<Dispatcher>();

                dispatcher.Dispatch(new AddAuditLogEntryCommand
                {
                    AuditLogEntry = new AuditLogEntryDTO
                    {
                        UserId = currentUser.IsAuthenticated ? currentUser.UserId : Guid.Empty,
                        CreatedDateTime = domainEvent.EventDateTime,
                        Action = "CREATED_PRODUCT",
                        ObjectId = domainEvent.Entity.Id.ToString(),
                        Log = domainEvent.Entity.AsJsonString(),
                    },
                });
            }
        }
    }
}