using System;
using System.Windows.Forms;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.Security;

namespace FuntinoControlCenter.Forms
{
    public partial class OperationsManagementForm : Form
    {
        public OperationsManagementForm()
        {
            InitializeComponent();
        }

        private void OperationsManagementForm_Load(object sender, EventArgs e)
        {
            try
            {
                ApplyPermissions();
                if (!HasManagementAccess())
                {
                    AppMessageService.ShowWarning(this, "برای مدیریت تنظیمات عملیاتی مجوزی ندارید.", "دسترسی محدود");
                    Close();
                    return;
                }
                AuthorizationMonitor.Attach(this, HasManagementAccess);
            }
            catch (Exception ex)
            {
                AppMessageService.ShowError(this, DatabaseManager.SafeMessage(ex), "بازکردن مدیریت عملیات");
                Close();
            }
        }

        private void ApplyPermissions()
        {
            btnAttractions.Visible = BusinessManagementRepository.HasAccess(BusinessManagementArea.Attractions, "view");
            btnControllerDevices.Visible = BusinessManagementRepository.HasAccess(BusinessManagementArea.ControllerDevices, "view");
            btnPromotions.Visible = BusinessManagementRepository.HasAccess(BusinessManagementArea.Promotions, "view");
            btnDevicePromotions.Visible = BusinessManagementRepository.HasAccess(BusinessManagementArea.DevicePromotions, "view");
            btnDiscountPolicies.Visible = BusinessManagementRepository.HasAccess(BusinessManagementArea.DiscountPolicies, "view");
            btnDiscountRates.Visible = BusinessManagementRepository.HasAccess(BusinessManagementArea.DiscountRates, "view");
            btnDeposits.Visible = BusinessManagementRepository.HasAccess(BusinessManagementArea.Deposits, "view");
            btnRechargeLimit.Visible = BusinessManagementRepository.HasAccess(BusinessManagementArea.RechargeLimit, "view");
            btnHourlyPlans.Visible = BusinessManagementRepository.HasAccess(BusinessManagementArea.HourlyPlans, "view");
            btnSchedules.Visible = BusinessManagementRepository.HasAccess(BusinessManagementArea.Schedules, "view");
            btnTickets.Visible = BusinessManagementRepository.HasAccess(BusinessManagementArea.Tickets, "view");
        }

        private bool HasManagementAccess()
        {
            return BusinessManagementRepository.HasAccess(BusinessManagementArea.Attractions, "view") ||
                BusinessManagementRepository.HasAccess(BusinessManagementArea.ControllerDevices, "view") ||
                BusinessManagementRepository.HasAccess(BusinessManagementArea.Promotions, "view") ||
                BusinessManagementRepository.HasAccess(BusinessManagementArea.DevicePromotions, "view") ||
                BusinessManagementRepository.HasAccess(BusinessManagementArea.DiscountPolicies, "view") ||
                BusinessManagementRepository.HasAccess(BusinessManagementArea.DiscountRates, "view") ||
                BusinessManagementRepository.HasAccess(BusinessManagementArea.Deposits, "view") ||
                BusinessManagementRepository.HasAccess(BusinessManagementArea.RechargeLimit, "view") ||
                BusinessManagementRepository.HasAccess(BusinessManagementArea.HourlyPlans, "view") ||
                BusinessManagementRepository.HasAccess(BusinessManagementArea.Schedules, "view") ||
                BusinessManagementRepository.HasAccess(BusinessManagementArea.Tickets, "view");
        }

        private void OpenArea(BusinessManagementArea area)
        {
            try
            {
                if (!BusinessManagementRepository.HasAccess(area, "view"))
                    throw new UnauthorizedAccessException("مجوز مشاهده‌ی این بخش را ندارید.");
                using (var form = new BusinessManagementForm(area)) form.ShowDialog(this);
                ApplyPermissions();
            }
            catch (Exception ex)
            {
                AppMessageService.ShowError(this, DatabaseManager.SafeMessage(ex), "مدیریت اطلاعات");
            }
        }

        private void Attractions_Click(object sender, EventArgs e) { OpenArea(BusinessManagementArea.Attractions); }
        private void ControllerDevices_Click(object sender, EventArgs e) { OpenArea(BusinessManagementArea.ControllerDevices); }
        private void Promotions_Click(object sender, EventArgs e) { OpenArea(BusinessManagementArea.Promotions); }
        private void DevicePromotions_Click(object sender, EventArgs e) { OpenArea(BusinessManagementArea.DevicePromotions); }
        private void DiscountPolicies_Click(object sender, EventArgs e) { OpenArea(BusinessManagementArea.DiscountPolicies); }
        private void DiscountRates_Click(object sender, EventArgs e) { OpenArea(BusinessManagementArea.DiscountRates); }
        private void Deposits_Click(object sender, EventArgs e) { OpenArea(BusinessManagementArea.Deposits); }
        private void RechargeLimit_Click(object sender, EventArgs e) { OpenArea(BusinessManagementArea.RechargeLimit); }
        private void HourlyPlans_Click(object sender, EventArgs e) { OpenArea(BusinessManagementArea.HourlyPlans); }
        private void Schedules_Click(object sender, EventArgs e) { OpenArea(BusinessManagementArea.Schedules); }
        private void Tickets_Click(object sender, EventArgs e) { OpenArea(BusinessManagementArea.Tickets); }
        private void Close_Click(object sender, EventArgs e) { Close(); }
    }
}
