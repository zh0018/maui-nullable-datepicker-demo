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

        // 标志：是否为用户主动打开的 Picker（只有为 true 时才接受 DateSelected）
        private bool _isUserOpeningPicker = false;

        public NullableDatePicker()
        {
            _label = new Label { VerticalOptions = LayoutOptions.Center };
            _datePicker = new DatePicker { IsVisible = false };

            // 只有当用户主动打开 picker（Focused）时，DateSelected 才会被视为用户选择
            _datePicker.Focused += (s, e) => _isUserOpeningPicker = true;
            _datePicker.Unfocused += (s, e) => _isUserOpeningPicker = false;

            _datePicker.DateSelected += (s, e) =>
            {
                // 忽略非用户触发的事件（例如控件初始化或程序性设置导致的事件）
                if (!_isUserOpeningPicker)
                    return;

                // 将用户选择的日期写回 SelectedDate
                SelectedDate = e.NewDate;

                // 选择完成后重置标志（Unfocused 也会重置，但这里更明确）
                _isUserOpeningPicker = false;
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

            // 只有在 SelectedDate 有值时，才把 DatePicker 的日期同步到已选择的日期。
            // 如果 SelectedDate 为 null，则不覆盖底层 DatePicker 的日期（避免误触发）
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
