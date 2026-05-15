using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace SysMan.Services.LDapServices
{
    public class ExchangeProvisioner
    {
        public (bool ok, string output) EnableMailboxOnPrem(string samAccountName, string dbName, string smtp)
        {
            var initial = InitialSessionState.CreateDefault();
            // Không gọi ImportPSModule("Exchange") nữa — không có module tên đó

            using var runspace = RunspaceFactory.CreateRunspace(initial);
            runspace.Open();

            using var ps = PowerShell.Create();
            ps.Runspace = runspace;

            // Script: nạp Exchange, rồi thao tác mailbox
            ps.AddScript($@"
        $ErrorActionPreference = 'Stop'

        # 1) Thử nạp môi trường Exchange theo cách chuẩn On-Prem
        $remoteExchange = Join-Path $env:ExchangeInstallPath 'bin\RemoteExchange.ps1'
        if (Test-Path $remoteExchange) {{
            . $remoteExchange
            Connect-ExchangeServer -Auto | Out-Null
        }}
        else {{
            # 2) Fallback: thêm snapin (tùy phiên bản)
            try {{ Add-PSSnapin Microsoft.Exchange.Management.PowerShell.SnapIn -ErrorAction Stop }} catch {{}}
            try {{ Add-PSSnapin Microsoft.Exchange.Management.PowerShell.E2010 -ErrorAction Stop }} catch {{}}
        }}

        # Kiểm tra cmdlet đã có chưa
        if (-not (Get-Command Get-User -ErrorAction SilentlyContinue)) {{
            throw 'Exchange cmdlets are not loaded. Open Exchange Management Shell or ensure EMS is installed.'
        }}

        # 3) Thao tác mailbox
        $u = Get-User -Identity '{samAccountName}'
        if (-not $u) {{ throw 'User not found in AD' }}

        $mbx = Get-Mailbox -Identity '{samAccountName}' -ErrorAction SilentlyContinue
        if (-not $mbx) {{
            Enable-Mailbox -Identity '{samAccountName}' -Database '{dbName}' -Alias '{samAccountName}'
        }}

        if ('{smtp}') {{
            Set-Mailbox -Identity '{samAccountName}' -PrimarySmtpAddress '{smtp}'
        }}

        'Mailbox enabled successfully for {samAccountName}'
    ");

            try
            {
                var results = ps.Invoke();
                var sb = new StringBuilder();
                foreach (var r in results) sb.AppendLine(r?.ToString());
                if (ps.HadErrors)
                {
                    var errs = string.Join("\n", ps.Streams.Error);
                    throw new Exception(errs);
                }
                return (true, sb.ToString().Trim());
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
