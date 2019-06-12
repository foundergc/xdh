using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using InPlanLib;

namespace PreCheckoutActions
{
    class Program
    {
        private ApplicationManager appManager;
        private IJobManager jobManager;
        private IJob job;
        private string jobName;

        static void Main(string[] args)
        {
            Program program = new Program();

            //登录InPlan
            program.Login();

            //检查是否允许checkin
            program.SetCheckin();
        }

        //登录InPlan
        private void Login()
        {
            try
            {
                appManager = new ApplicationManager();
                jobManager = appManager.JobManager();

                if (Environment.GetCommandLineArgs().Length == 2)
                {
                    jobName = Environment.GetCommandLineArgs()[1]; //通过Inplan run script调用
                }
                else if (Environment.GetCommandLineArgs().Length == 3)
                {
                    jobName = Environment.GetCommandLineArgs()[2]; //通过Inplan menu第2个参数是"-job"
                }
                else if (Environment.GetCommandLineArgs().Length == 4)
                {
                    string user = Environment.GetCommandLineArgs()[1];
                    string pwd = Environment.GetCommandLineArgs()[2];
                    jobName = Environment.GetCommandLineArgs()[3];
                    appManager.Login(user, pwd);
                }

                job = jobManager.OpenJob(jobName);
                if (appManager.ErrorStatus() != 0)
                {
                    MessageBox.Show(appManager.ErrorMessage(), "Error", MessageBoxButtons.OK);
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not Create JobInfo Manager!/n" + ex.Message, "Error!");
                Application.Exit();
            }
        }

        private void SetCheckin()
        {
            bool editable = true;
            bool miFinished = false;

            string sql = $@"
                SELECT LTRIM(RTRIM(allow_edit_flag)) editable
                FROM data0050 
                WHERE customer_part_number = '{jobName}'
                ";
            SqlDataReader dataReader = DBHelper.GetDataReader(sql);
            while (dataReader.Read())
            {
                if (dataReader["editable"].ToString() == "N")
                {
                    editable = false;
                    break;
                }
            }

            sql = $@"
                SELECT
	                LTRIM(RTRIM(MI完成知会时间)) mi_time,
	                LTRIM(RTRIM(BOM表完成知会时间)) bom_time
                FROM gc_data001 
                WHERE 生产部件 = '{jobName}'
                ";

            dataReader = DBHelper.GetDataReader(sql);
            while (dataReader.Read())
            {
                if (dataReader["mi_time"].ToString() != "")
                {
                    miFinished = true;
                    break;
                }
            }

            if (!editable && miFinished)
            {
                MessageBox.Show("此料号ERP已暂停编辑且MI已审核!\n\n修改后必须解锁并重新上传!","警告",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
            else if (!editable)
            {
                MessageBox.Show("此料号ERP已暂停编辑!\n\n修改后必须解锁并重新上传!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (miFinished)
            {
                MessageBox.Show("此料号MI已审核!\n\n修改后必须解锁并重新上传!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (job.NeedRollToLatest())
            {
                jobManager.SaveJob(job);
                appManager.RollAllToLatest(job);
                if (appManager.ErrorStatus() > 0)
                    MessageBox.Show("Checkout时出错了,以下为错误详细信息!\n" + appManager.ErrorMessage());
            }

            appManager.CheckOutAll(job, "");
            if (appManager.ErrorStatus() > 0)
                MessageBox.Show("Checkout时出错了,以下为错误详细信息!\n" + appManager.ErrorMessage());
            jobManager.CloseJob(job);
            jobManager.OpenJob(jobName);
        }

    }
}
