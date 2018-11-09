namespace WindowsServiceBIMJCI
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.serviceProcessInstallerJCI = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceInstallerJCI = new System.ServiceProcess.ServiceInstaller();
            // 
            // serviceProcessInstallerJCI
            // 
            this.serviceProcessInstallerJCI.Account = System.ServiceProcess.ServiceAccount.LocalService;
            this.serviceProcessInstallerJCI.Password = null;
            this.serviceProcessInstallerJCI.Username = null;
            // 
            // serviceInstallerJCI
            // 
            this.serviceInstallerJCI.Description = "BIM JCI参数值读取";
            this.serviceInstallerJCI.DisplayName = "BIM JCI参数值读取";
            this.serviceInstallerJCI.ServiceName = "BIMJCIService";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceProcessInstallerJCI,
            this.serviceInstallerJCI});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstallerJCI;
        private System.ServiceProcess.ServiceInstaller serviceInstallerJCI;
    }
}