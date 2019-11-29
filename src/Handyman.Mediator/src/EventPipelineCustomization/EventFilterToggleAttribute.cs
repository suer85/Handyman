﻿using System;

namespace Handyman.Mediator.EventPipelineCustomization
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class EventFilterToggleAttribute : EventPipelineBuilderAttribute
    {
        private readonly Type _toggleEnabledFilterType;

        public EventFilterToggleAttribute(Type toggleEnabledFilterType)
        {
            _toggleEnabledFilterType = toggleEnabledFilterType ?? throw new ArgumentNullException(nameof(toggleEnabledFilterType));
        }

        public Type ToggleDisabledFilterType { get; set; }

        public override void Configure<TEvent>(IEventPipelineBuilder<TEvent> builder, ServiceProvider serviceProvider)
        {
            builder.AddFilterSelector(new EventFilterToggleFilterSelector<TEvent>(_toggleEnabledFilterType)
            {
                ToggleDisabledFilterType = ToggleDisabledFilterType
            });
        }
    }
}