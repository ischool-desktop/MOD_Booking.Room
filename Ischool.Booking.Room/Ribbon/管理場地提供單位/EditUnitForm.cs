﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FISCA.Presentation.Controls;
using FISCA.Data;
using K12.Data;

namespace Ischool.Booking.Room
{
    public partial class EditUnitForm : BaseForm
    {
        /// <summary>
        /// 編輯模式: 新增or修改
        /// </summary>
        string _mode;
        /// <summary>
        /// 管理單位編號
        /// </summary>
        string _unitID;

        public EditUnitForm(string mode,string unitID)
        {
            InitializeComponent();

            _mode = mode;

            _unitID = unitID;

            if (mode == "新增")
            {
                dateLb.Text = "日期:   " + DateTime.Now.ToShortDateString();

                ReloadDataGridview();
            }
            else if (mode == "修改")
            {
                string sql = string.Format(@"
SELECT
    unit.name AS unit_name
    , teacher.teacher_name
    , unit.create_time
FROM
    $ischool.booking.meetingroom_unit AS unit
    LEFT OUTER JOIN $ischool.booking.meetingroom_unit_admin AS unit_admin
        ON unit.uid = unit_admin.ref_unit_id
        AND unit_admin.is_boss = 'true'
    LEFT OUTER JOIN teacher
        ON unit_admin.ref_teacher_id = teacher.id
WHERE
    unit.uid = {0}
                    ", unitID);
                QueryHelper qh = new QueryHelper();
                DataTable dt = qh.Select(sql);

                unitNameTbx.Text = "" + dt.Rows[0]["unit_name"];
                unitBossTbx.Text = "" + dt.Rows[0]["teacher_name"];
                dateLb.Text = "日期:   " + DateTime.Parse("" + dt.Rows[0]["create_time"]).ToShortDateString();

                ReloadDataGridview();
            }
            
        }

        public void ReloadDataGridview()
        {
            // 取得未指定系統管理員的老師清單
            string sql = @"
SELECT 
    teacher.* 
FROM 
    teacher 
    LEFT OUTER JOIN $ischool.booking.meetingroom_system_admin AS system_admin
        ON teacher.id = system_admin.ref_teacher_id
    LEFT OUTER JOIN $ischool.booking.meetingroom_unit_admin AS unit_admin
        ON teacher.id = unit_admin.ref_teacher_id
WHERE 
    status = 1
    AND system_admin.uid IS NULL
    AND unit_admin.uid IS NULL
";
            QueryHelper qh = new QueryHelper();
            DataTable dt = qh.Select(sql);

            foreach (DataRow row in dt.Rows)
            {
                DataGridViewRow datarow = new DataGridViewRow();
                datarow.CreateCells(dataGridViewX1);

                int index = 0;

                string gender = "";
                switch ("" + row["gender"])
                {
                    case "0":
                        gender = "女";
                        break;
                    case "1":
                        gender = "男";
                        break;
                    default:
                        gender = "";
                        break;
                }

                datarow.Cells[index++].Value = "" + row["teacher_name"];
                datarow.Cells[index++].Value = "" + row["nickname"];
                datarow.Cells[index++].Value = gender;
                datarow.Cells[index++].Value = "" + row["st_login_name"];
                datarow.Cells[index++].Value = "" + row["dept"];
                datarow.Cells[index++].Value = "指定";
                datarow.Tag = "" + row["id"];   // 老師系統編號
                dataGridViewX1.Rows.Add(datarow);
            }
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            #region 資料驗證
            if (unitNameTbx.Text == "")
            {
                errorLb.Text = "請輸入單位名稱!";
                errorLb.Visible = true;
                return;
            }
            else if (unitBossTbx.Text == "")
            {
                errorLb.Text = "請選擇單位主管!";
                errorLb.Visible = true;
                return;
            }
            #endregion

            // 根據模式調整SQL內容
            #region SQL

            string unitName = unitNameTbx.Text;
            string createTime = DateTime.Now.ToShortDateString();
            string createdBy = Actor.Account;
            string adminAccount = "" + ((DataGridViewRow)unitBossTbx.Tag).Cells[3].Value;
            string refTeacherID = "" + ((DataGridViewRow)unitBossTbx.Tag).Tag;
            string isBoss = "true";

            string sql = "";

            if (_mode == "新增")
            {
                #region sql

                sql = string.Format(@"
WITH data_row AS(
    SELECT
        '{0}'::TEXT AS unit_name
        , '{1}'::TIMESTAMP AS create_time
        , '{2}'::TEXT AS created_by
        , '{3}'::TEXT AS admin_account
        , {4}::BIGINT AS ref_teacher_id
        , '{5}'::BOOLEAN AS is_boss
) ,insert_unit_data AS(
    INSERT INTO $ischool.booking.meetingroom_unit(
        name
        , create_time
        , created_by
    )
    SELECT
        unit_name
        , create_time
        , created_by
    FROM
        data_row

    RETURNING $ischool.booking.meetingroom_unit.*
)
INSERT INTO $ischool.booking.meetingroom_unit_admin(
    account
    , ref_unit_id
    , ref_teacher_id
    , is_boss
    , create_time
    , created_by
)
SELECT
    data_row.admin_account
    , insert_unit_data.uid
    , data_row.ref_teacher_id
    , data_row.is_boss
    , data_row.create_time
    , data_row.created_by
FROM
    data_row
    LEFT OUTER JOIN insert_unit_data
        ON data_row.unit_name = insert_unit_data.name
                ", unitName, createTime, createdBy, adminAccount, refTeacherID, isBoss);

                #endregion

            }
            else if(_mode == "修改")
            {
                #region sql

                sql = string.Format(@"
WITH data_row AS(
    SELECT
        '{0}'::TEXT AS unit_name
        , {1}::BIGINT AS unit_id
        , '{2}'::TEXT AS created_by
        , '{3}'::TEXT AS account
        , '{4}'::BIGINT AS ref_teacher_id
        , '{5}'::BOOLEAN AS is_boss
        , '{6}'::TIMESTAMP AS create_time
) ,update_unit_data AS(
    UPDATE
        $ischool.booking.meetingroom_unit
    SET
        name = data_row.unit_name
    FROM
        data_row
    WHERE
        uid = data_row.unit_id
) ,delete_unit_admin AS(
    DELETE
    FROM
        $ischool.booking.meetingroom_unit_admin
    WHERE
        uid = (SELECT unit_id FROM data_row )
)
INSERT INTO $ischool.booking.meetingroom_unit_admin(
    account
    , ref_unit_id
    , ref_teacher_id
    , is_boss
    , create_time
    , created_by
)    
SELECT
    data_row.account
    , data_row.unit_id
    , data_row.ref_teacher_id
    , data_row.is_boss
    , data_row.create_time
    , data_row.created_by
FROM
    data_row
                    ", unitName, _unitID, createdBy, adminAccount, refTeacherID, isBoss, createTime);

                #endregion
            }

            #endregion

            UpdateHelper up = new UpdateHelper();
            try
            {
                up.Execute(sql);
                MessageBox.Show("儲存成功!");
                this.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            
        }

        private void leaveBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void searchTbx_TextChanged(object sender, EventArgs e)
        {
            if (searchTbx.Text == "")
            {
                foreach (DataGridViewRow row in dataGridViewX1.Rows)
                {
                    row.Visible = true;
                }
            }
            else
            {
                foreach (DataGridViewRow row in dataGridViewX1.Rows)
                {
                    if ("" + row.Cells[0].Value == searchTbx.Text)
                    {
                        row.Visible = true;
                    }
                    else
                    {
                        row.Visible = false;
                    }
                }
            }
        }

        private void dataGridViewX1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int col = e.ColumnIndex;
            int row = e.RowIndex;
            if (col == 5)
            {
                unitBossTbx.Text = "" + dataGridViewX1.Rows[row].Cells[0].Value;
                unitBossTbx.Tag = dataGridViewX1.Rows[row];  // datarow
            }
        }
    }
}
