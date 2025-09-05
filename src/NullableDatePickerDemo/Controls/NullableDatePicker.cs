using System;
using Microsoft.Maui.Controls;

namespace NullableDatePickerDemo.Controls
{
    public class NullableDatePicker : ContentView
    {
        public static readonly BindableProperty SelectedDateProperty =
            BindableProperty.Create(
                nameof(SelectedDate),
                typeof(DateTime?),
                typeof(NullableDatePicker),
                null,
                BindingMode.TwoWay,
                propertyChanged: OnSelectedDateChanged);

        public DateTime? SelectedDate
        {
            get => (DateTime?)GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }

        public static readonly BindableProperty FormatProperty =
            BindableProperty.Create(
                nameof(Format),
                typeof(string),
                typeof(NullableDatePicker),
                "d");

        public string Format
        {
            get => (string)GetValue(FormatProperty);
            set => SetValue(FormatProperty, value);
        }

        private readonly DatePicker _datePicker;
        private readonly Label _label;
        private readonly Button _clearButton;
        private readonly Button _pickButton;

        public NullableDatePicker()
        {
            _label = new Label { VerticalOptions = LayoutOptions.Center };
            _datePicker = new DatePicker { IsVisible = false };
            _datePicker.DateSelected += (s, e) =>
            {
                SelectedDate = e.NewDate;
            };

            _pickButton = new Button { Text = "Choose date" };
            _pickButton.Clicked += (s, e) => _datePicker.Focus();

            _clearButton = new Button { Text = "Clear", IsEnabled = false };
            _clearButton.Clicked += (s, e) => SelectedDate = null;

            var stack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { _label, _pickButton, _clearButton, _datePicker }
            };

            Content = stack;

            UpdateLabel();
        }

        private static void OnSelectedDateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (NullableDatePicker)bindable;
            control.UpdateLabel();
            control._clearButton.IsEnabled = control.SelectedDate.HasValue;
            if (control.SelectedDate.HasValue)
            {
                control._datePicker.Date = control.SelectedDate.Value;
            }
        }

        private void UpdateLabel()
        {
            _label.Text = SelectedDate?.ToString(Format) ?? "No date selected";
        }
    }
}