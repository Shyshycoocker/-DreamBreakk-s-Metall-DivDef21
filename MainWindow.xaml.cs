using DevExpress.DataAccess.Native.Data;
using DevExpress.Dialogs.Core.View;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraVerticalGrid;
using DXDivDef.ViewModels;
using DXDivDef.Views;
using Oracle.ManagedDataAccess.Client;
using ReportDataTables;
using System;
using System.Collections.Generic;
using System.Data;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DXDivDef
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        System.Data.DataTable argsDT = new System.Data.DataTable();
        System.Data.DataTable ProductDT = new System.Data.DataTable(); 
        System.Data.DataTable ordersDT = new System.Data.DataTable();
        System.Data.DataTable GridDT = new System.Data.DataTable();
        System.Data.DataTable nullDT = new System.Data.DataTable();
        ProductsViewModel prdmodel = new ProductsViewModel();
        GetDataTables NewData = new GetDataTables();
        OracleConnection connection;
        OracleCommand command;
        MainViewModel vm = new MainViewModel();
        string sPath, sNote;
        public MainWindow()
        {
            CultureInfo culture = CultureInfo.CreateSpecificCulture("ru-RU");

            // The following line provides localization for the application's user interface. 
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("ru");

            // The following line provides localization for data formats. 
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");

            // Set this culture as the default culture for all threads in this application. 
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CreateSpecificCulture("ru");

            InitializeComponent();

            var OraConn = NewData.OraDBConn(1, true);
            connection = OraConn.Item1;
            command = OraConn.Item2;

            argsDT = NewData.GetARGS(connection, command);
            argsDT.Clear();
            argsDT.Rows.Add("КМ11550101500", "012813-500157", "212", "01", "01", "2024", null, null, null, null, "DivDef", "DivDef", 10);
            ProductDT = GetProductsDataTable(connection, command, argsDT);
            prdmodel.SetProducts(ProductDT);
            PrdComboBox.DataContext = prdmodel;

            //var ss = vm.Source;
            //tGridControl.ItemsSource = typeViewModel;

        }
        public System.Data.DataTable GetOrdersDataTable(OracleConnection oracleConnection, OracleCommand oracleCommand, System.Data.DataTable argsDT)
        {
            System.Data.DataTable dt = new System.Data.DataTable();

            oracleCommand.Connection = oracleConnection;
            oracleCommand.CommandText = "" +
                "SELECT  DISTINCT\r\n" +
                "        cpb.id          AS \"Order\"\r\n" +
                "    ,   COALESCE(REGEXP_SUBSTR(\r\n" +
                "                REGEXP_SUBSTR(cpb.name, '\\d+(\\s?шт)+', 1, 1)\r\n" +
                "                    , '\\d+\\'), '1')\r\n" +
                "                        AS OrdCount\r\n" +
                "FROM\r\n" +
                "        cost_point_objects cpo\r\n" +
                "INNER JOIN\r\n" +
                "        cost_point cp\r\n" +
                "ON\r\n" +
                "        cp.list_id = cpo.list_id\r\n" +
                "    AND cp.id = cpo.id\r\n" +
                "INNER JOIN\r\n" +
                "        cost_point cpb\r\n" +
                "ON\r\n" +
                "        cpb.list_id = 'ЗАК'\r\n" +
                "    AND cpb.id = cp.spec_3_value\r\n" +
                "WHERE\r\n" +
                "        cpo.list_id = 'ЗАКП'\r\n" +
               $"    AND cpo.obj_id = '{argsDT.Rows[0][0]}'\r\n" +
                "    AND (\r\n" +
                "                cpb.spec_11_value IS NULL\r\n" +
                "            OR  TO_DATE(cpb.spec_10_value) > SYSDATE\r\n" +
                "        )";
            using (OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(command))
            {
                oracleDataAdapter.Fill(dt);
            }
            return dt;
        }
        public void UpdInsNote(OracleConnection oracleConnection, OracleCommand oracleCommand)
        {
            oracleCommand.Connection = oracleConnection;
            oracleCommand.CommandText = "" +
                /*"UPDATE\r\n" +
                "       metex.def21_orders_note\r\n " +
                "SET\r\n" +
               $"       snote = '{sNote}'\r\n" +
                "WHERE\r\n" +
               $"       spath = '{sPath}'\r\n" +
               $"   AND sorders IN\r\n" +
                "                   (\r\n" +
                "                       SELECT\r\n" +
               $"                               REGEXP_SUBSTR('-'||'{argsDT.Rows[0][1]}', '([^-]+)', 1, LEVEL) orders\r\n" +
                "                       FROM\r\n" +
                "                               dual\r\n" +
               $"                       CONNECT BY LEVEL <= REGEXP_COUNT('-'||'{argsDT.Rows[0][1]}', '[^-]+')\r\n" +
                "                   )";*/
                "BEGIN\r\n" +
                "    FOR rec IN\r\n" +
                "        (\r\n" +
                "            SELECT\r\n" +
                "                    s.spath\r\n" +
                "                ,   s.sorders\r\n" +
                "                ,   s.snote\r\n" +
                "                ,   CASE\r\n" +
                "                        WHEN LENGTH(s.snote) > 0\r\n" +
                "                        THEN    CASE\r\n" +
                "                                    WHEN don.spath IS NULL\r\n" +
                "                                    THEN 1\r\n" +
                "                                    ELSE 2\r\n" +
                "                                END\r\n" +
                "                        ELSE 0\r\n" +
                "                    END flAction\r\n" +
                "            FROM\r\n" +
                "                (\r\n" +
                "                    SELECT\r\n" +
               $"                            '{sPath}' spath\r\n" +
               $"                        ,   REGEXP_SUBSTR('-'||'{argsDT.Rows[0][1]}', '([^-]+)', 1, LEVEL) sorders\r\n" +
               $"                        ,   '{sNote}'  snote\r\n" +
                "                    FROM\r\n" +
                "                            dual\r\n" +
               $"                    CONNECT BY LEVEL <= REGEXP_COUNT('-'||'{argsDT.Rows[0][1]}', '[^-]+')\r\n" +
                "                ) s\r\n" +
                "            LEFT JOIN\r\n" +
                "                    metex.def21_orders_note don\r\n" +
                "            ON\r\n" +
                "                    don.spath = s.spath\r\n" +
                "                AND s.sorders = don.sorders\r\n" +
                "        )\r\n" +
                "    LOOP\r\n" +
                "        IF( rec.flAction = 0) THEN\r\n" +
                "            DELETE FROM\r\n" +
                "                    metex.def21_orders_note\r\n" +
                "            WHERE\r\n" +
                "                    spath = rec.spath\r\n" +
                "                AND sorders = rec.sorders;\r\n" +
                "        ELSIF (rec.flAction = 1) THEN\r\n" +
                "            INSERT INTO\r\n" +
                "                    metex.def21_orders_note\r\n" +
                "                (\r\n" +
                "                        spath\r\n" +
                "                    ,   sorders\r\n" +
                "                    ,   snote\r\n" +
                "                )\r\n" +
                "            VALUES\r\n" +
                "                (\r\n" +
                "                        rec.spath\r\n" +
                "                    ,   rec.sorders\r\n" +
                "                    ,   rec.snote\r\n" +
                "                );\r\n" +
                "        ELSIF (rec.flAction = 2) THEN\r\n" +
                "            UPDATE\r\n" +
                "                    metex.def21_orders_note\r\n" +
                "            SET\r\n" +
                "                    snote = rec.snote\r\n" +
                "            WHERE\r\n" +
                "                    spath = rec.spath\r\n" +
                "                AND sorders = rec.sorders;\r\n" +
                "        END IF;\r\n" +
                "    END LOOP;\r\n" +
                "END;";
            oracleCommand.ExecuteNonQuery();
        }
        public System.Data.DataTable GetProductsDataTable(OracleConnection oracleConnection, OracleCommand oracleCommand, System.Data.DataTable argsDT)
        {
            System.Data.DataTable dt = new System.Data.DataTable();

            oracleCommand.Connection = oracleConnection;
            oracleCommand.CommandText = "" +
                "SELECT  DISTINCT\r\n" +
                "        o.id\r\n" +
                "    ,   o.id||' - '||o.name \"PrdIDName\"\r\n" +
                "FROM\r\n" +
                "        object o   \r\n" +
                "INNER JOIN    \r\n" +
                "    (\r\n" +
                "        SELECT  DISTINCT\r\n" +
                "                prd_id\r\n" +
                "        FROM\r\n" +
                "                metex.current_tree\r\n" +
                "    ) s \r\n" +
                "ON        \r\n" +
                "        s.prd_id = o.id\r\n" +
                "WHERE\r\n" +
                "        o.list_id = 'САГП'";
            using (OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(command))
            {
                oracleDataAdapter.Fill(dt);
            }
            return dt;
        }
        public System.Data.DataTable GetMainDataTable(OracleConnection oracleConnection, OracleCommand oracleCommand, System.Data.DataTable argsDT)
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            oracleCommand.Connection = oracleConnection;
            oracleCommand.CommandText = "" +
        
                "TRUNCATE TABLE metex.tmp_def21_tree";
            oracleCommand.ExecuteNonQuery();

            oracleCommand.Connection = oracleConnection;
            oracleCommand.CommandText = "" +

                "INSERT INTO\r\n" +
                "        metex.tmp_def21_tree\r\n" +
                "SELECT\r\n" +
                "        *\r\n" +
                "FROM\r\n" +
                "    (\r\n" +
                "        WITH\r\n" +
                "                ctr\r\n" +
                "        AS  (\r\n" +
                "                SELECT\r\n" +
                "                        *\r\n" +
                "                FROM\r\n" +
                "                        metex.current_tree ct\r\n" +
                "                WHERE\r\n" +
                "                        prd_list_id = 'ДСЕ'\r\n" +
               $"                    AND prd_id = '{argsDT.Rows[0][0]}'\r\n" +
                "            )\r\n" +
                "            ,   analog\r\n" +
                "        AS  (\r\n" +
                "                SELECT\r\n" +
                "                        ctr.*\r\n" +
                "                    ,   ctr.obj_list_id     AS obj_list_idz\r\n" +
                "                    ,   ctr.obj_id          AS obj_idz\r\n" +
                "                FROM\r\n" +
                "                        ctr\r\n" +
                "                UNION ALL\r\n" +
                "                SELECT\r\n" +
                "                        ctr.spath||'.ANALOG'||an.\"Аналог\"||'.ANALOG'\r\n" +
                "                                            AS spath\r\n" +
                "                    ,   ctr.prd_list_id\r\n" +
                "                    ,   ctr.prd_id\r\n" +
                "                    ,   ctr.parent_list_id\r\n" +
                "                    ,   ctr.parent_obj_id\r\n" +
                "                    ,   an.\"Спр.Аналог\"     AS obj_list_id\r\n" +
                "                    ,   an.\"Аналог\"         AS obj_id\r\n" +
                "                    ,   ctr.qty_1\r\n" +
                "                    ,   ctr.obj_list_id     AS obj_list_idz\r\n" +
                "                    ,   ctr.obj_id          AS obj_idz\r\n" +
                "                FROM\r\n" +
                "                        ctr\r\n" +
                "                INNER JOIN\r\n" +
                "                    (\r\n" +
                "                        SELECT\r\n" +
                "                                *\r\n" +
                "                        FROM\r\n" +
                "                            (\r\n" +
                "                                SELECT\r\n" +
                "                                        di2.i_anal_2_param1     AS \"Спр.ДСЕ\"\r\n" +
                "                                    ,   di2.i_anal_2_id         AS \"ДСЕ\"\r\n" +
                "                                    ,   di3.i_anal_2_param1     AS \"Спр.Аналог\"\r\n" +
                "                                    ,   di3.i_anal_2_id         AS \"Аналог\"\r\n" +
                "                                    ,   d.doc_id\r\n" +
                "                                    ,   s2.oper_datetime\r\n" +
                "                                    ,   MAX(s2.oper_datetime)\r\n" +
                "                                            OVER (PARTITION BY di2.i_anal_2_id)\r\n" +
                "                                                                AS mOper_datetime\r\n" +
                "                                FROM\r\n" +
                "                                    (\r\n" +
                "                                        SELECT  DISTINCT\r\n" +
                "                                               ct.prd_list_id\r\n" +
                "                                            ,   ct.prd_id\r\n" +
                "                                        FROM\r\n" +
                "                                                metex.current_tree ct\r\n" +
                "                                        WHERE\r\n" +
                "                                                EXISTS\r\n" +
                "                                                    (\r\n" +
                "                                                        SELECT\r\n" +
                "                                                                1\r\n" +
                "                                                        FROM\r\n" +
                "                                                                ctr cte\r\n" +
                "                                                        INNER JOIN\r\n" +
                "                                                                object_product_t opt\r\n" +
                "                                                        ON\r\n" +
                "                                                                opt.list_id = cte.obj_list_id\r\n" +
                "                                                            AND opt.id = cte.obj_id\r\n" +
                "                                                            AND opt.prd_kind_id = 'Сб'\r\n" +
                "                                                        WHERE\r\n" +
                "                                                                cte.obj_list_id = ct.obj_list_id\r\n" +
                "                                                            AND cte.obj_id = ct.obj_id\r\n" +
                "                                                    )\r\n" +
                "                                            AND ct.parent_list_id IS NULL\r\n" +
                "                                            AND ct.parent_obj_id IS NULL\r\n" +
                "                                        ORDER BY\r\n" +
                "                                                ct.prd_id\r\n" +
                "                                    ) s\r\n" +
                "                                INNER JOIN\r\n" +
                "                                        doc d\r\n" +
                "                                ON\r\n" +
                "                                        d.is_delete = 0\r\n" +
                "                                    AND d.dt_id = 'СПЕЦ'\r\n" +
                "                                    AND d.dtv_id = 'СпецИзвЗ'\r\n" +
                "                                    AND d.h_anal_2_id = 'Д'\r\n" +
                "                                    AND d.status = '1000000000'\r\n" +
                "                                    AND d.h_anal_1_param1 = s.prd_list_id\r\n" +
                "                                    AND d.h_anal_1_id = s.prd_id\r\n" +
                "                                INNER JOIN\r\n" +
                "                                        doc_item di1\r\n" +
                "                                ON\r\n" +
                "                                        d.doc_id = di1.doc_id\r\n" +
                "                                    AND di1.tbl_no = 1\r\n" +
                "                                INNER JOIN\r\n" +
                "                                        doc_item di2\r\n" +
                "                                ON\r\n" +
                "                                        di1.doc_id = di2.doc_id\r\n" +
                "                                    AND di1.item_rec_id = di2.parent_item_rec_id\r\n" +
                "                                    AND di2.tbl_no = 6\r\n" +
                "                                INNER JOIN\r\n" +
                "                                        doc_item di3\r\n" +
                "                                ON\r\n" +
                "                                        di1.doc_id = di3.doc_id\r\n" +
                "                                    AND di1.item_rec_id = di3.parent_item_rec_id\r\n" +
                "                                    AND di3.tbl_no = 7\r\n" +
                "                                INNER JOIN\r\n" +
                "                                    (\r\n" +
                "                                        SELECT\r\n" +
                "                                                doc_id\r\n" +
                "                                            ,   oper_datetime\r\n" +
                "                                        FROM\r\n" +
                "                                                doc_oper\r\n" +
                "                                        WHERE\r\n" +
                "                                                initial_status = '0000000000'\r\n" +
                "                                            AND final_status LIKE '1%'\r\n" +
                "                                    ) s2\r\n" +
                "                                ON\r\n" +
                "                                        s2.doc_id = d.doc_id\r\n" +
                "                            )\r\n" +
                "                        WHERE\r\n" +
                "                                oper_datetime = mOper_datetime\r\n" +
                "                    ) an\r\n" +
                "                ON\r\n" +
                "                        ctr.obj_list_id = an.\"Спр.ДСЕ\"\r\n" +
                "                    AND ctr.obj_id = an.\"ДСЕ\"\r\n" +
                "            )\r\n" +
                "            ,   orders\r\n" +
                "        AS  (\r\n" +
                "                SELECT\r\n" +
               $"                        REGEXP_SUBSTR('-'||'{argsDT.Rows[0][1]}', '([^-]+)', 1, LEVEL)\r\n" +
                "                                        AS sOrder\r\n" +
                "                    ,   1               AS fl\r\n" +
                "                FROM\r\n" +
                "                         dual\r\n" +
               $"                CONNECT BY LEVEL <= REGEXP_COUNT('-'||'{argsDT.Rows[0][1]}', '[^-]+')\r\n" +
                "            )\r\n" +
                "            ,   ost\r\n" +
                "        AS (\r\n" +
                "                SELECT\r\n" +
                "                        ibd.anal_id\r\n" +
                "                    ,   ibd.entry_year\r\n" +
                "                    ,   ibd.entry_month\r\n" +
                "                    ,   ibd.entry_day\r\n" +
                "                    ,   ibd.qty_1_in\r\n" +
                "                    ,   CASE\r\n" +
                "                            WHEN entry_day > 0\r\n" +
                "                            THEN ibd.qty_1_rcp\r\n" +
                "                            ELSE 0\r\n" +
                "                        END qty_1_rcp\r\n" +
                "                    ,      CASE\r\n" +
                "                            WHEN entry_day > 0\r\n" +
                "                            THEN ibd.qty_1_exp\r\n" +
                "                            ELSE 0\r\n" +
                "                        END qty_1_exp\r\n" +
                "                    ,   ibd.qty_1_out\r\n" +
                "                    ,   ibd.qty_1_inv\r\n" +
                "                    ,   iba.div_1_id\r\n" +
                "                    ,   iba.cp_1_id\r\n" +
                "                    ,   iba.obj_1_id\r\n" +
                "                    ,   iba.obj_1_list_id\r\n" +
                "                    ,   iba.btt_id\r\n" +
                "                FROM\r\n" +
                "                        inv_bt_anal iba\r\n" +
                "                JOIN\r\n" +
                "                        inv_bt_data ibd\r\n" +
                "                ON\r\n" +
                "                        iba.anal_id = ibd.anal_id\r\n" +
                "                WHERE\r\n" +
                "                        iba.btt_id IN ('ДСЕНМК', 'ЦехМАТ')\r\n" +
                "                    AND EXISTS\r\n" +
                "                        (\r\n" +
                "                            SELECT\r\n" +
                "                                    1\r\n" +
                "                            FROM\r\n" +
                "                                    analog de\r\n" +
                "                            WHERE\r\n" +
                "                                    de.obj_list_id = iba.obj_1_list_id\r\n" +
                "                                AND de.obj_id = iba.obj_1_id\r\n" +
                "                        )\r\n" +
                "                    AND EXISTS\r\n" +
                "                        (\r\n" +
                "                            SELECT\r\n" +
                "                                    1\r\n" +
                "                            FROM\r\n" +
                "                                    orders orde\r\n" +
                "                            WHERE\r\n" +
                "                                    orde.sOrder = iba.cp_1_id\r\n" +
                "                        )\r\n" +
               $"                    AND ibd.entry_year = '{argsDT.Rows[0][5]}'\r\n" +
               $"                    AND ibd.entry_month = '{argsDT.Rows[0][4]}'\r\n" +
               $"                    AND ibd.entry_day <= '{argsDT.Rows[0][3]}'\r\n" +
                "        )\r\n" +
                "            ,   ost_inv_date\r\n" +
                "        AS (\r\n" +
                "                SELECT\r\n" +
                "                        *\r\n" +
                "                FROM\r\n" +
                "                    (\r\n" +
                "                        SELECT\r\n" +
                "                                s.*\r\n" +
                "                            ,   ROW_NUMBER()\r\n" +
                "                                    OVER (PARTITION BY anal_id ORDER BY entry_date DESC) rwn\r\n" +
                "                        FROM\r\n" +
                "                            (\r\n" +
                "                                SELECT\r\n" +
                "                                        anal_id\r\n" +
                "                                    ,   qty_1\r\n" +
                "                                    ,   qty_1_in\r\n" +
                "                                    ,   MAX(entry_date) OVER (PARTITION BY anal_id) entry_date\r\n" +
                "                                    ,   obj_1_list_id\r\n" +
                "                                    ,   obj_1_id\r\n" +
                "                                    ,   div_1_id\r\n" +
                "                                    ,   entry_day\r\n" +
                "                                FROM\r\n" +
                "                                    (\r\n" +
                "                                        SELECT\r\n" +
                "                                                *\r\n" +
                "                                        FROM\r\n" +
                "                                            (\r\n" +
                "                                                SELECT\r\n" +
                "                                                        ibe.anal_id\r\n" +
                "                                                    ,   ibe.entry_date\r\n" +
                "                                                    ,   ibe.qty_1\r\n" +
                "                                                    ,   o.qty_1_in\r\n" +
                "                                                    ,   o.entry_day\r\n" +
                "                                                    ,   ibe.doc_oper_datetime\r\n" +
                "                                                    ,   o.obj_1_list_id\r\n" +
                "                                                    ,   o.obj_1_id\r\n" +
                "                                                    ,   o.div_1_id\r\n" +
                "                                                    ,   ROW_NUMBER()\r\n" +
                "                                                            OVER (PARTITION BY o.obj_1_list_id, o.obj_1_id, ibe.oper_no, ibe.entry_action, ibe.doc_item_no\r\n" +
                "                                                                ORDER BY o.obj_1_list_id, o.obj_1_id, ibe.oper_no, ibe.entry_action, ibe.doc_item_no DESC) rwn\r\n" +
                "                                                FROM\r\n" +
                "                                                        ost o\r\n" +
                "                                                INNER JOIN\r\n" +
                "                                                        inv_bte ibe\r\n" +
                "                                                ON\r\n" +
                "                                                        o.anal_id = ibe.anal_id\r\n" +
                "                                                WHERE\r\n" +
                "                                                        ibe.entry_action = 'И'\r\n" +
                "                                                    AND o.qty_1_inv IS NOT NULL\r\n" +
                "                                                    AND ibe.entry_status != 1\r\n" +
                "                                            )\r\n" +
                "                                        WHERE\r\n" +
                "                                                rwn = 1\r\n" +
                "                                    )\r\n" +
                "                            ) s\r\n" +
                "                    ) s\r\n" +
                "                WHERE\r\n" +
                "                        rwn = 1\r\n" +
                "            )\r\n" +
                "            ,   ost_filter_inv\r\n" +
                "        AS  (\r\n" +
                "                SELECT\r\n" +
                "                        qty_1\r\n" +
                "                    ,   anal_id\r\n" +
                "                    ,   entry_date\r\n" +
                "                    ,   CASE\r\n" +
                "                            WHEN NVL(div_1_id, 0) != '03803'\r\n" +
                "                                AND NVL(div_1_id, 0) = div\r\n" +
                "                                OR  (NVL(div_1_id, 0) != '03803' OR s.obj_list_id = 'ПКИ')\r\n" +
                "                            THEN s.qty_1\r\n" +
                "                            ELSE 0\r\n" +
                "                        END\r\n" +
                "                        +   NVL(s.qty_1_cex_rcp, 0) - NVL(s.qty_1_cex_exp, 0)\r\n" +
                "                                        AS qty_1_cex_cgp\r\n" +
                "                    ,   CASE\r\n" +
                "                            WHEN NVL(div_1_id, 0) != '03803'\r\n" +
                "                                AND NVL(div_1_id, 0) != div\r\n" +
                "                                OR  (NVL(div_1_id, 0) != '03803' OR s.obj_list_id = 'ПКИ')\r\n" +
                "                            THEN s.qty_1\r\n" +
                "                            ELSE 0\r\n" +
                "                        END\r\n" +
                "                        +   NVL(s.qty_1_cex_rcp, 0) - NVL(s.qty_1_cex_exp, 0)\r\n" +
                "                                        AS qty_1_anth_cex_cgp\r\n" +
                "                    ,   CASE\r\n" +
                "                            WHEN NVL(div_1_id, 0) = '03803'\r\n" +
                "                            THEN s.qty_1\r\n" +
                "                            ELSE 0\r\n" +
                "                        END\r\n" +
                "                        +   NVL(s.qty_1_sgd_rcp, 0) - NVL(s.qty_1_sgd_exp, 0)\r\n" +
                "                                        AS qty_1_sgd_cgp\r\n" +
                "                FROM\r\n" +
                "                    (\r\n" +
                "                        SELECT\r\n" +
                "                                qty_1\r\n" +
                "                            ,   qty_1_in\r\n" +
                "                            ,   anal_id\r\n" +
                "                            ,   entry_date\r\n" +
                "                            ,   div_1_id\r\n" +
                "                            ,   entry_day\r\n" +
                "                            ,   div\r\n" +
                "                            ,   obj_list_id\r\n" +
                "                            ,   CASE\r\n" +
                "                                    WHEN NVL(div_1_id, 0) != '03803'\r\n" +
                "                                    THEN SUM(   CASE\r\n" +
                "                                                    WHEN entry_action = 'П'\r\n" +
                "                                                    THEN qty_1_ibe\r\n" +
                "                                                    ELSE 0\r\n" +
                "                                                END\r\n" +
                "                                            )\r\n" +
                "                                            OVER (PARTITION BY anal_id)\r\n" +
                "                                END             AS qty_1_cex_rcp\r\n" +
                "                            ,   CASE\r\n" +
                "                                    WHEN NVL(div_1_id, 0) != '03803'\r\n" +
                "                                    THEN SUM(   CASE\r\n" +
                "                                                    WHEN entry_action = 'Р'\r\n" +
                "                                                    THEN qty_1_ibe\r\n" +
                "                                                    ELSE 0\r\n" +
                "                                                END\r\n" +
                "                                            )\r\n" +
                "                                            OVER (PARTITION BY anal_id)\r\n" +
                "                                END             AS qty_1_cex_exp\r\n" +
                "                            ,   CASE\r\n" +
                "                                    WHEN NVL(div_1_id, 0) = '03803'\r\n" +
                "                                    THEN SUM(   CASE\r\n" +
                "                                                    WHEN entry_action = 'П'\r\n" +
                "                                                    THEN qty_1_ibe\r\n" +
                "                                                    ELSE 0\r\n" +
                "                                                END\r\n" +
                "                                            )\r\n" +
                "                                            OVER (PARTITION BY anal_id)\r\n" +
                "                                END             AS qty_1_sgd_rcp\r\n" +
                "                            ,   CASE\r\n" +
                "                                    WHEN NVL(div_1_id, 0) = '03803'\r\n" +
                "                                    THEN SUM(   CASE\r\n" +
                "                                                    WHEN entry_action = 'Р'\r\n" +
                "                                                    THEN qty_1_ibe\r\n" +
                "                                                    ELSE 0\r\n" +
                "                                                END\r\n" +
                "                                            )\r\n" +
                "                                            OVER (PARTITION BY anal_id)\r\n" +
                "                                END             AS qty_1_sgd_exp\r\n" +
                "                            ,   ROW_NUMBER()\r\n" +
                "                                    OVER (PARTITION BY anal_id ORDER BY ed1 DESC)\r\n" +
                "                                                AS rwn\r\n" +
                "                        FROM\r\n" +
                "                            (\r\n" +
                "                                SELECT\r\n" +
                "                                        s.qty_1\r\n" +
                "                                    ,   qty_1_in\r\n" +
                "                                    ,   anal_id\r\n" +
                "                                    ,   entry_date\r\n" +
                "                                    ,   div_1_id\r\n" +
                "                                    ,   entry_day\r\n" +
                "                                    ,   div\r\n" +
                "                                    ,   obj_list_id\r\n" +
                "                                    ,   entry_date ed1\r\n" +
                "                                    ,   entry_action\r\n" +
                "                                    ,   s.qty_1 qty_1_ibe\r\n" +
                "                                FROM\r\n" +
                "                                    (\r\n" +
                "                                        SELECT\r\n" +
                "                                                ido.qty_1\r\n" +
                "                                            ,   ido.qty_1_in\r\n" +
                "                                            ,   ibe.anal_id\r\n" +
                "                                            ,   ido.entry_date\r\n" +
                "                                            ,   div_1_id\r\n" +
                "                                            ,   entry_day\r\n" +
                "                                            ,   ido.obj_1_list_id\r\n" +
                "                                            ,   ido.obj_1_id\r\n" +
                "                                            ,   ibe.entry_date ed1\r\n" +
                "                                            ,   entry_action\r\n" +
                "                                            ,   ibe.qty_1 qty_1_ibe\r\n" +
                "                                        FROM\r\n" +
                "                                                ost_inv_date ido\r\n" +
                "                                        INNER JOIN\r\n" +
                "                                                inv_bte ibe\r\n" +
                "                                        ON\r\n" +
                "                                                ibe.anal_id = ido.anal_id\r\n" +
                "                                            AND ibe.entry_date >= ido.entry_date\r\n" +
               $"                                            AND ibe.entry_date <= TO_DATE('{argsDT.Rows[0][3]}.{argsDT.Rows[0][4]}.{argsDT.Rows[0][5]}')\r\n" +
                "                                            AND entry_status != 1\r\n" +
                "                                    ) s\r\n" +
                "                                INNER JOIN\r\n" +
                "                                    (\r\n" +
                "                                        SELECT\r\n" +
                "                                                obj_list_id\r\n" +
                "                                            ,   obj_id\r\n" +
                "                                            ,   '021'   div\r\n" +
                "                                        FROM\r\n" +
                "                                                analog\r\n" +
                "                                        GROUP BY\r\n" +
                "                                                obj_list_id\r\n" +
                "                                            ,   obj_id\r\n" +
                "            --                                    ,   div\r\n" +
                "                                    ) d\r\n" +
                "                                ON\r\n" +
                "                                        d.obj_list_id = s.obj_1_list_id\r\n" +
                "                                    AND d.obj_id = s.obj_1_id\r\n" +
                "                            ) s\r\n" +
                "                    ) s\r\n" +
                "                WHERE\r\n" +
                "                        rwn = 1\r\n" +
                "            )\r\n" +
                "            ,   ost_date\r\n" +
                "        AS (\r\n" +
                "                SELECT\r\n" +
                "                        *\r\n" +
                "                FROM\r\n" +
                "                    (\r\n" +
                "                        SELECT\r\n" +
                "                                anal_id\r\n" +
                "                            ,   entry_date\r\n" +
                "                            ,   qty_1\r\n" +
                "                            ,   qty_1_in\r\n" +
                "                            ,   entry_day\r\n" +
                "                            ,   ROW_NUMBER()\r\n" +
                "                                    OVER (PARTITION BY anal_id ORDER BY entry_date DESC)\r\n" +
                "                                            AS rwn\r\n" +
                "                            ,   obj_1_list_id\r\n" +
                "                            ,   obj_1_id\r\n" +
                "                            ,   btt_id\r\n" +
                "                        FROM\r\n" +
                "                            (\r\n" +
                "                                SELECT\r\n" +
                "                                        ibe.anal_id\r\n" +
                "                                    ,   ibe.entry_date\r\n" +
                "                                    ,   qty_1\r\n" +
                "                                    ,   qty_1_in\r\n" +
                "                                    ,   entry_day\r\n" +
                "                                    ,   ROW_NUMBER()\r\n" +
                "                                            OVER (PARTITION BY obj_1_list_id, obj_1_id, oper_no, entry_action, doc_item_no\r\n" +
                "                                                ORDER BY obj_1_list_id, obj_1_id, oper_no, entry_action, doc_item_no DESC)\r\n" +
                "                                                    AS  rwn\r\n" +
                "                                    ,   obj_1_list_id\r\n" +
                "                                    ,   obj_1_id\r\n" +
                "                                    ,   o.btt_id\r\n" +
                "                                FROM\r\n" +
                "                                        ost o\r\n" +
                "                                INNER JOIN\r\n" +
                "                                        inv_bte ibe\r\n" +
                "                                ON\r\n" +
                "                                        o.anal_id = ibe.anal_id\r\n" +
                "                                WHERE\r\n" +
                "                                        entry_action != 'И'\r\n" +
                "                                    AND entry_status != 1\r\n" +
                "                            ) s\r\n                        WHERE\r\n" +
                "                                rwn = 1\r\n" +
                "                    )\r\n" +
                "                WHERE\r\n" +
                "                        rwn = 1\r\n" +
                "            )\r\n" +
                "            ,   ost_filter0\r\n" +
                "        AS (\r\n" +
                "                SELECT\r\n" +
                "                        qty_1_rcp\r\n" +
                "                    ,   qty_1_exp\r\n" +
                "                    ,   qty_1\r\n" +
                "                    ,   anal_id\r\n" +
                "                FROM\r\n" +
                "                    (\r\n" +
                "                        SELECT\r\n" +
                "                                SUM(    CASE\r\n" +
                "                                            WHEN entry_action = 'П'\r\n" +
                "                                            THEN qty_1\r\n" +
                "                                            ELSE 0\r\n" +
                "                                        END\r\n" +
                "                                    )\r\n" +
                "                                    OVER (PARTITION BY anal_id)\r\n" +
                "                                        AS qty_1_rcp\r\n" +
                "                            ,   SUM(    CASE\r\n" +
                "                                            WHEN entry_action = 'Р'\r\n" +
                "                                            THEN qty_1\r\n" +
                "                                            ELSE 0\r\n" +
                "                                        END\r\n" +
                "                                    )\r\n" +
                "                                    OVER (PARTITION BY anal_id) qty_1_exp\r\n" +
                "                            ,   qty_1\r\n" +
                "                            ,   anal_id\r\n" +
                "                            ,   ROW_NUMBER()\r\n" +
                "                                    OVER (PARTITION BY anal_id ORDER BY doc_oper_datetime DESC)\r\n" +
                "                                                AS rwn\r\n" +
                "                        FROM\r\n" +
                "                            (\r\n" +
                "                                SELECT\r\n" +
                "                                        ibe.qty_1\r\n" +
                "                                    ,   ibe.anal_id\r\n" +
                "                                    ,   ibe.doc_oper_datetime\r\n" +
                "                                    ,   entry_action\r\n" +
                "                                    ,   ROW_NUMBER()\r\n" +
                "                                            OVER (PARTITION BY ibe.anal_id, obj_1_list_id, obj_1_id, oper_no, entry_action, doc_item_no\r\n" +
                "                                                ORDER BY ibe.anal_id, obj_1_list_id, obj_1_id, oper_no, entry_action, doc_item_no DESC)\r\n" +
                "                                                        AS rwn\r\n" +
                "                                FROM\r\n" +
                "                                        ost_date ido\r\n" +
                "                                JOIN\r\n" +
                "                                        inv_bte ibe\r\n" +
                "                                ON\r\n" +
                "                                        ido.anal_id = ibe.anal_id\r\n" +
                "                                WHERE\r\n" +
               $"                                        ibe.entry_date <= TO_DATE('{argsDT.Rows[0][3]}.{argsDT.Rows[0][4]}.{argsDT.Rows[0][5]}')\r\n" +
                "                                    AND entry_status != 1\r\n" +
                "                            )\r\n" +
                "                        WHERE\r\n" +
                "                                rwn = 1\r\n" +
                "                    )\r\n" +
                "                WHERE\r\n" +
                "                        rwn = 1\r\n" +
                "            )\r\n" +
                "            ,   ost_number\r\n" +
                "        AS (\r\n" +
                "                SELECT\r\n" +
                "                        qty_1_cex\r\n" +
                "                    ,   qty_1_anth_cex\r\n" +
                "                    ,   qty_1_sgd\r\n" +
                "                    ,   qty_1_rcp\r\n" +
                "                    ,   qty_1_exp\r\n" +
                "                    ,   anal_id\r\n" +
                "                    ,   div_1_id\r\n" +
                "                    ,   cp_1_id\r\n" +
                "                    ,   qty_1_in_sgd\r\n" +
                "                    ,   qty_1_in_cex\r\n" +
                "                    ,   obj_list_id\r\n" +
                "                    ,   obj_id\r\n" +
                "                    ,   entry_date\r\n" +
                "                    ,   btt_id\r\n" +
                "                    ,   entry_day\r\n" +
                "                FROM\r\n" +
                "                    (\r\n" +
                "                        SELECT DISTINCT\r\n" +
                "                                o.qty_1_in\r\n" +
                "                            ,   CASE\r\n" +
               $"                                    WHEN NVL(osfi.entry_date, TO_DATE('01.01.1900')) <= TO_DATE('{argsDT.Rows[0][3]}.{argsDT.Rows[0][4]}.{argsDT.Rows[0][5]}')\r\n" +
                "                                        AND entry_day > 0\r\n" +
                "                                    THEN o.qty_1_rcp\r\n" +
                "                                    ELSE 0\r\n" +
                "                                END qty_1_rcp\r\n" +
                "                            ,   CASE\r\n" +
               $"                                    WHEN NVL(osfi.entry_date, TO_DATE('01.01.1900')) <= TO_DATE('{argsDT.Rows[0][3]}.{argsDT.Rows[0][4]}.{argsDT.Rows[0][5]}')\r\n" +
                "                                        AND entry_day > 0\r\n" +
                "                                    THEN o.qty_1_exp\r\n" +
                "                                    ELSE 0\r\n" +
                "                                END qty_1_exp\r\n" +
                "                            ,   CASE\r\n" +
                "                                    WHEN qty_1_cex_cgp IS NOT NULL\r\n" +
                "                                    THEN qty_1_cex_cgp\r\n" +
                "                                    ELSE\r\n" +
                "                                        CASE\r\n" +
                "                                            WHEN (NVL(div_1_id, 0) != '03803')\r\n" +
                "                                                OR (    NVL(div_1_id, 0) != '03803'\r\n" +
                "                                                    AND O.obj_1_list_id = 'ПКИ')\r\n" +
                "                                            THEN    CASE\r\n" +
                "                                                        WHEN qty_1_in = NVL(osf.qty_1_rcp, 0) - NVL(osf.qty_1_exp, 0)\r\n" +
                "                                                        THEN qty_1_in\r\n" +
                "                                                        ELSE NVL(osf.qty_1_rcp, 0) - NVL(osf.qty_1_exp, 0)\r\n" +
                "                                                    END\r\n" +
                "                                            ELSE 0\r\n" +
                "                                        END\r\n" +
                "                                END             AS qty_1_cex\r\n" +
                "                            ,   CASE\r\n" +
                "                                    WHEN qty_1_anth_cex_cgp IS NOT NULL\r\n" +
                "                                    THEN qty_1_anth_cex_cgp\r\n" +
                "                                    ELSE\r\n" +
                "                                        CASE\r\n" +
                "                                            WHEN (NVL(div_1_id, 0) != '03803')\r\n" +
                "                                                OR (    NVL(div_1_id, 0) != '03803'\r\n" +
                "                                                    AND O.obj_1_list_id = 'ПКИ')\r\n" +
                "                                            THEN    CASE\r\n" +
                "                                                        WHEN qty_1_in = NVL(osf.qty_1_rcp, 0) - NVL(osf.qty_1_exp, 0)\r\n" +
                "                                                        THEN qty_1_in\r\n" +
                "                                                        ELSE NVL(osf.qty_1_rcp, 0) - NVL(osf.qty_1_exp, 0)\r\n" +
                "                                                    END\r\n" +
                "                                            ELSE 0\r\n" +
                "                                        END\r\n" +
                "                                END             AS qty_1_anth_cex\r\n" +
                "                            ,   CASE\r\n" +
                "                                    WHEN qty_1_sgd_cgp IS NOT NULL\r\n" +
                "                                    THEN qty_1_sgd_cgp\r\n" +
                "                                    ELSE    CASE\r\n" +
                "                                                WHEN NVL(div_1_id, 0) = '03803'\r\n" +
                "                                                THEN    CASE\r\n" +
                "                                                            WHEN qty_1_in = NVL(osf.qty_1_rcp, 0) - NVL(osf.qty_1_exp, 0)\r\n" +
                "                                                            THEN qty_1_in\r\n" +
                "                                                            ELSE NVL(osf.qty_1_rcp, 0) - NVL(osf.qty_1_exp, 0)\r\n" +
                "                                                        END\r\n" +
                "                                                ELSE 0\r\n" +
                "                                            END\r\n" +
                "                                END             AS qty_1_sgd\r\n" +
                "                            ,   entry_day\r\n" +
                "                            ,   o.anal_id\r\n" +
                "                            ,   div_1_id\r\n" +
                "                            ,   cp_1_id\r\n" +
                "                            ,   CASE\r\n" +
                "                                    WHEN NVL(div_1_id, 0) = '03803'\r\n" +
                "                                        AND entry_day <= 1\r\n" +
                "                                    THEN qty_1_in\r\n" +
                "                                    ELSE 0\r\n" +
                "                                END             AS qty_1_in_sgd\r\n" +
                "                            ,   CASE\r\n" +
                "                                    WHEN NVL(div_1_id, 0) != '03803'\r\n" +
                "                                        AND entry_day <= 1\r\n" +
                "                                    THEN qty_1_in\r\n" +
                "                                    ELSE 0\r\n" +
                "                                END             AS qty_1_in_cex\r\n" +
                "                            ,   entry_date\r\n" +
                "                            ,   obj_1_list_id obj_list_id\r\n" +
                "                            ,   obj_1_id        AS obj_id\r\n" +
                "                            ,   o.btt_id\r\n" +
                "                        FROM\r\n" +
                "                                ost o\r\n" +
                "                        LEFT JOIN\r\n" +
                "                                ost_filter_inv osfi\r\n" +
                "                        ON\r\n" +
                "                                o.anal_id = osfi.anal_id\r\n" +
                "                        JOIN\r\n" +
                "                                ost_filter0 osf\r\n" +
                "                        ON\r\n" +
                "                                o.anal_id = osf.anal_id\r\n" +
                "                        WHERE\r\n" +
               $"                                entry_year = '{argsDT.Rows[0][5]}'\r\n" +
               $"                            AND entry_month = '{argsDT.Rows[0][4]}'\r\n" +
               $"                            AND entry_day <= '{argsDT.Rows[0][3]}'\r\n" +
                "                    )\r\n" +
                "            )\r\n" +
                "            ,   cgp\r\n" +
                "        AS (\r\n" +
                "                SELECT\r\n" +
                "                        \"СГД\"\r\n" +
                "                    ,   CASE\r\n" +
                "                            WHEN \"ПРИХОД\"+\"РАСХОД\" = 0\r\n" +
                "                            THEN \"1 ЦЕХ\"\r\n" +
                "                            ELSE \"ЦЕХ\"\r\n" +
                "                        END \"ЦЕХ\"\r\n" +
                "                    ,   \"ДРУГИЕ ЦЕХА\"\r\n" +
                "                    ,   \"ПРИХОД\"\r\n" +
                "                    ,   \"РАСХОД\"\r\n" +
                "                    ,   \"1 СГД\"\r\n" +
                "                    ,   \"1 ЦЕХ\"\r\n" +
                "                    ,   \"1 ДРУГИЕ ЦЕХА\"\r\n" +
                "                    ,   obj_list_id\r\n" +
                "                    ,   obj_id\r\n" +
                "                    ,   anal_id\r\n" +
                "                    ,   entry_date\r\n" +
                "                FROM\r\n" +
                "                    (\r\n" +
                "                        SELECT\r\n" +
                "                                NVL(SUM(    CASE\r\n" +
                "                                                WHEN entry_day > 0\r\n" +
                "                                                THEN qty_1_sgd\r\n" +
                "                                                ELSE 0\r\n" +
                "                                            END), 0)\r\n" +
                "                                                AS \"СГД\"\r\n" +
                "                            ,   NVL(SUM(    CASE\r\n" +
                "                                                WHEN div_1_id = '021'\r\n" +
                "                                                    AND entry_day > 0\r\n" +
                "                                                THEN qty_1_cex\r\n" +
                "                                                ELSE 0\r\n" +
                "                                            END\r\n" +
                "                                        ), 0)   AS \"ЦЕХ\"\r\n" +
                "                            ,   NVL(SUM(    CASE\r\n" +
                "                                                WHEN div_1_id != '021'\r\n" +
                "                                                    AND btt_id = 'ДСЕНМК'\r\n" +
                "                                                    AND entry_day > 0\r\n" +
                "                                                THEN qty_1_anth_cex\r\n" +
                "                                                ELSE 0\r\n" +
                "                                            END\r\n" +
                "                                        ), 0)   AS \"ДРУГИЕ ЦЕХА\"\r\n" +
                "                            ,   NVL(SUM(    CASE\r\n" +
                "                                                WHEN div_1_id = '021'\r\n" +
                "                                                    AND entry_day > 0\r\n" +
                "                                                THEN qty_1_rcp\r\n" +
                "                                                ELSE 0\r\n" +
                "                                            END\r\n" +
                "                                        ), 0)   AS \"ПРИХОД\"\r\n" +
                "                            ,   NVL(SUM(    CASE\r\n" +
                "                                                WHEN div_1_id = '021'\r\n" +
                "                                                    AND entry_day > 0\r\n" +
                "                                                THEN qty_1_exp\r\n" +
                "                                                ELSE 0\r\n" +
                "                                            END\r\n" +
                "                                        ), 0)   AS \"РАСХОД\"\r\n" +
                "                            ,   NVL(SUM(    CASE\r\n" +
                "                                                WHEN entry_day = 0\r\n" +
                "                                                THEN qty_1_in_sgd\r\n" +
                "                                                ELSE 0\r\n" +
                "                                            END), 0)\r\n" +
                "                                                AS \"1 СГД\"\r\n" +
                "                            ,   NVL(SUM(    CASE\r\n" +
                "                                                WHEN div_1_id = '021'\r\n" +
                "                                                    AND entry_day = 0\r\n" +
                "                                                THEN qty_1_in_cex\r\n" +
                "                                                ELSE 0\r\n" +
                "                                            END\r\n" +
                "                                        ), 0)   AS \"1 ЦЕХ\"\r\n" +
                "                            ,   NVL(SUM(    CASE\r\n" +
                "                                                WHEN div_1_id != '021'\r\n" +
                "                                                    AND entry_day = 0\r\n" +
                "                                                THEN qty_1_in_cex\r\n" +
                "                                                ELSE 0\r\n" +
                "                                            END\r\n" +
                "                                        ), 0)   AS \"1 ДРУГИЕ ЦЕХА\"\r\n" +
                "                            ,   obj_list_id\r\n" +
                "                            ,   obj_id\r\n" +
                "                            ,   anal_id\r\n" +
                "                            ,   entry_date\r\n" +
                "                        FROM\r\n" +
                "                            (\r\n" +
                "                                SELECT\r\n" +
                "                                        CASE\r\n" +
                "                                            WHEN cnt1 = 0\r\n" +
                "                                            THEN 0\r\n" +
                "                                            ELSE qty_1_sgd\r\n" +
                "                                        END             AS qty_1_sgd\r\n" +
                "                                    ,   CASE\r\n" +
                "                                            WHEN cnt2 = 0\r\n" +
                "                                            THEN 0\r\n" +
                "                                            ELSE qty_1_cex\r\n" +
                "                                        END             AS qty_1_cex\r\n" +
                "                                    ,   CASE\r\n" +
                "                                            WHEN cnt5 = 0\r\n" +
                "                                            THEN 0\r\n" +
                "                                            ELSE qty_1_anth_cex\r\n" +
                "                                        END             AS qty_1_anth_cex\r\n" +
                "                                    ,   CASE\r\n" +
                "                                            WHEN cnt6 = 0\r\n" +
                "                                            THEN 0\r\n" +
                "                                            ELSE qty_1_rcp\r\n" +
                "                                        END             AS qty_1_rcp\r\n" +
                "                                    ,   CASE\r\n" +
                "                                            WHEN cnt7 = 0\r\n" +
                "                                            THEN 0\r\n" +
                "                                            ELSE qty_1_exp\r\n" +
                "                                        END             AS qty_1_exp\r\n" +
                "                                    ,   CASE\r\n" +
                "                                            WHEN cnt3 = 0\r\n" +
                "                                            THEN 0\r\n" +
                "                                            ELSE qty_1_in_sgd\r\n" +
                "                                        END             AS qty_1_in_sgd\r\n" +
                "                                    ,   CASE\r\n" +
                "                                            WHEN cnt4 = 0\r\n" +
                "                                            THEN 0\r\n" +
                "                                            ELSE qty_1_in_cex\r\n" +
                "                                        END             AS qty_1_in_cex\r\n" +
                "                                    ,   cp_1_id\r\n" +
                "                                    ,   obj_list_id\r\n" +
                "                                    ,   obj_id\r\n" +
                "                                    ,   anal_id\r\n" +
                "                                    ,   entry_date\r\n" +
                "                                    ,   div_1_id\r\n" +
                "                                    ,   btt_id\r\n" +
                "                                    ,   entry_day\r\n" +
                "                                FROM\r\n" +
                "                                    (\r\n" +
                "                                        SELECT\r\n" +
                "                                                COUNT(qty_1_sgd)\r\n" +
                "                                                    OVER (PARTITION BY anal_id)\r\n" +
                "                                                                AS cnt1\r\n" +
                "                                            ,   COUNT(qty_1_cex)\r\n" +
                "                                                    OVER (PARTITION BY anal_id)\r\n" +
                "                                                                AS cnt2\r\n" +
                "                                            ,   COUNT(qty_1_anth_cex)\r\n" +
                "                                                    OVER (PARTITION BY anal_id)\r\n" +
                "                                                                AS cnt5\r\n" +
                "                                            ,   COUNT(qty_1_rcp)\r\n" +
                "                                                    OVER (PARTITION BY anal_id)\r\n" +
                "                                                                AS cnt6\r\n" +
                "                                            ,   COUNT(qty_1_exp)\r\n" +
                "                                                    OVER (PARTITION BY anal_id)\r\n" +
                "                                                                AS cnt7\r\n" +
                "                                            ,   COUNT(qty_1_in_sgd)\r\n" +
                "                                                    OVER (PARTITION BY anal_id)\r\n" +
                "                                                                AS cnt3\r\n" +
                "                                            ,   COUNT(qty_1_in_cex)\r\n" +
                "                                                    OVER (PARTITION BY anal_id)\r\n" +
                "                                                                AS cnt4\r\n" +
                "                                            ,   qty_1_sgd\r\n" +
                "                                            ,   qty_1_cex\r\n" +
                "                                            ,   qty_1_anth_cex\r\n" +
                "                                            ,   qty_1_rcp\r\n" +
                "                                            ,   qty_1_exp\r\n" +
                "                                            ,   qty_1_in_sgd\r\n" +
                "                                            ,   qty_1_in_cex\r\n" +
                "                                            ,   cp_1_id\r\n" +
                "                                            ,   obj_list_id\r\n" +
                "                                            ,   obj_id\r\n" +
                "                                            ,   anal_id\r\n" +
                "                                            ,   NVL((   SELECT\r\n" +
                "                                                                MAX(entry_date)\r\n" +
                "                                                        FROM\r\n" +
                "                                                                inv_bte\r\n" +
                "                                                        WHERE\r\n" +
                "                                                                anal_id = os.anal_id\r\n" +
                "                                                            AND entry_action = 'И'\r\n" +
                "                                                    ), '01.01.1999')\r\n" +
                "                                                                AS entry_date\r\n" +
                "                                            ,   div_1_id\r\n" +
                "                                            ,   btt_id\r\n" +
                "                                            ,   entry_day\r\n" +
                "                                        FROM\r\n" +
                "                                                ost_number os\r\n" +
                "                                    )\r\n" +
                "                            ) s\r\n" +
                "                        GROUP BY\r\n" +
                "                                obj_list_id\r\n" +
                "                            ,   obj_id\r\n" +
                "                            ,   anal_id\r\n" +
                "                            ,   entry_date\r\n" +
                "                    )\r\n" +
                "            )\r\n" +
                "            ,   cgp1\r\n" +
                "        AS  (\r\n" +
                "                SELECT\r\n" +
                "                        obj_list_id\r\n" +
                "                    ,   obj_id\r\n" +
                "                    ,   \"1 СГД\"\r\n" +
                "                    ,   \"1 ЦЕХ\"\r\n" +
                "                    ,   \"СГД\"\r\n" +
                "                    ,   \"ПРИХОД\"\r\n" +
                "                    ,   \"РАСХОД\"\r\n" +
                "                    ,   \"ЦЕХ\"\r\n" +
                "                    ,  \"ДРУГИЕ ЦЕХА\"\r\n" +
                "                    ,   NVL((   SELECT\r\n" +
                "                                        MAX(entry_date)\r\n" +
                "                                FROM\r\n" +
                "                                        inv_bte\r\n" +
                "                                WHERE\r\n" +
                "                                        anal_id = s.anal_id\r\n" +
                "                                    AND entry_action = 'И'\r\n" +
                "                            ), '01.01.1999')\r\n" +
                "                                        AS entry_date\r\n" +
                "                FROM\r\n" +
                "                    (\r\n" +
                "                        SELECT\r\n" +
                "                                obj_list_id\r\n" +
                "                            ,   obj_id\r\n" +
                "                            ,   anal_id\r\n" +
                "                            ,   SUM(\"1 СГД\")\r\n" +
                "                                    OVER (PARTITION BY obj_list_id, obj_id)\r\n" +
                "                                                AS \"1 СГД\"\r\n" +
                "                            ,   SUM(\"1 ЦЕХ\")\r\n" +
                "                                    OVER (PARTITION BY obj_list_id, obj_id)\r\n" +
                "                                                AS \"1 ЦЕХ\"\r\n" +
                "                            ,   SUM(\"СГД\")\r\n" +
                "                                    OVER (PARTITION BY obj_list_id, obj_id)\r\n" +
                "                                                AS \"СГД\"\r\n" +
                "                            ,   SUM(\"ЦЕХ\")\r\n" +
                "                                    OVER (PARTITION BY obj_list_id, obj_id)\r\n" +
                "                                                AS \"ЦЕХ\"\r\n" +
                "                            ,   SUM(\"ДРУГИЕ ЦЕХА\")\r\n" +
                "                                    OVER (PARTITION BY obj_list_id, obj_id)\r\n" +
                "                                                AS \"ДРУГИЕ ЦЕХА\"\r\n" +
                "                            ,   SUM(\"ПРИХОД\")\r\n" +
                "                                    OVER (PARTITION BY obj_list_id, obj_id)\r\n" +
                "                                                AS \"ПРИХОД\"\r\n" +
                "                            ,   SUM(\"РАСХОД\")\r\n" +
                "                                    OVER (PARTITION BY obj_list_id, obj_id)\r\n" +
                "                                                AS \"РАСХОД\"\r\n" +
                "                            ,   ROW_NUMBER()\r\n" +
                "                                    OVER (PARTITION BY obj_list_id, obj_id\r\n" +
                "                                        ORDER BY obj_list_id, obj_id, anal_id)\r\n" +
                "                                                AS rwn\r\n" +
                "        --                    ,   div_1_id\r\n" +
                "                        FROM\r\n" +
                "                                cgp c\r\n" +
                "                    ) s\r\n" +
                "                WHERE\r\n" +
                "                        rwn = 1\r\n" +
                "                ORDER BY\r\n" +
                "                        obj_id      ASC\r\n" +
                "            )\r\n" +
                "        SELECT\r\n" +
                "                rownum          AS \"№ П/П\"\r\n" +
                "            ,   \"Системный номер\"\r\n" +
                "            ,   \"Компонент\"\r\n" +
                "            ,   \"Количество на 1 единицу\"\r\n" +
                "            ,   RowNo\r\n" +
                "            ,   Parent\r\n" +
                "            ,   PlanOnOrder\r\n" +
                "            ,   QtyNeed\r\n" +
                "            ,   BalanceOnFstDay\r\n" +
                "            ,   QtyComing\r\n" +
                "            ,   QtyOutGo\r\n" +
                "            ,   \"BalanceSRPUSX\"\r\n" +
                "            ,   sNote\r\n" +
                "            ,   \"ДРУГИЕ ЦЕХА\"\r\n" +
                "            ,   obj_list_id\r\n" +
                "            ,   obj_id\r\n" +
                "        FROM\r\n" +
                "            (\r\n" +
                "                SELECT\r\n" +
                "                        o.spec_6_value              AS \"Системный номер\"\r\n" +
                "                    ,   o.id\r\n" +
                "                        ||' - '\r\n" +
                "                        ||o.name\r\n" +
                "                        ||  CASE\r\n" +
                "                                WHEN SUBSTR(ctr.spath, LENGTH(ctr.spath) - 6, 7) = '.ANALOG'\r\n" +
                "                                THEN CHR(10)\r\n" +
                "                                    ||'Замена '\r\n" +
                "                                    ||ctr.obj_idz\r\n" +
                "                                ELSE ''\r\n" +
                "                            END                     AS \"Компонент\"\r\n" +
                "                    ,   ctr.qty_1                   AS \"Количество на 1 единицу\"\r\n" +
                "                    ,   ctr.spath                   AS RowNo\r\n" +
                "                    ,   pctr.spath                  AS Parent\r\n" +
                "                    ,   ctr.prd_id\r\n" +
               $"                    ,   NVL('{argsDT.Rows[0][2]}', 0)               AS PlanOnOrder\r\n" +
                "                    ,   NVL(ctr.qty_1\r\n" +
               $"                        *   TO_NUMBER('{argsDT.Rows[0][2]}'), 0)    AS QtyNeed\r\n" +
                "                    ,   NVL(c.\"1 ЦЕХ\", 0)           AS BalanceOnFstDay\r\n" +
                "                    ,   NVL(c.\"ПРИХОД\", 0)          AS QtyComing\r\n" +
                "                    ,   NVL(c.\"РАСХОД\", 0)          AS QtyOutGo\r\n" +
                "                    ,   c.\"СГД\"\r\n" +
                "                    ,   s.\"УСХ\"                     AS \"УСХ\"\r\n" +
                "                    ,   NVL(c.\"СГД\", 0)\r\n" +
                "                        + NVL(s.\"УСХ\", 0)           AS \"BalanceSRPUSX\"\r\n" +
                "                    ,   c.\"ДРУГИЕ ЦЕХА\"\r\n" +
                "                    ,   d21.snote\r\n" +
                "                    ,   ctr.obj_list_id\r\n" +
                "                    ,   ctr.obj_id\r\n" +
                "                FROM\r\n" +
                "                        analog ctr\r\n" +
                "                LEFT JOIN\r\n" +
                "                        analog pctr\r\n" +
                "                ON\r\n" +
                "                        pctr.spath = SUBSTR(ctr.spath, 1, INSTR(ctr.spath, '#', -1, 1) - 1)\r\n" +
                "                INNER JOIN\r\n" +
                "                        object o\r\n" +
                "                ON\r\n" +
                "                        o.list_id = ctr.obj_list_id\r\n" +
                "                    AND o.id = ctr.obj_id\r\n" +
                "                LEFT JOIN\r\n" +
                "                        cgp1 c\r\n" +
                "                ON\r\n" +
                "                        c.obj_list_id = ctr.obj_list_id\r\n" +
                "                    AND c.obj_id = ctr.obj_id\r\n" +
                "                LEFT JOIN\r\n" +
                "                        metex.def21_orders_note d21\r\n" +
                "                ON\r\n" +
                "                        d21.spath = ctr.spath\r\n" +
                "                    AND EXISTS\r\n" +
                "                        (\r\n" +
                "                            SELECT\r\n" +
                "                                    1\r\n" +
                "                            FROM\r\n" +
                "                                    orders\r\n" +
                "                            WHERE\r\n" +
                "                                    sOrder = d21.sorders\r\n" +
                "                        )\r\n" +
                "                LEFT JOIN\r\n" +
                "                    (\r\n" +
                "                        SELECT\r\n" +
                "                                iba.obj_1_list_id\r\n" +
                "                            ,   iba.obj_1_id\r\n" +
                "                            ,   SUM(NVL(qty_1_out, 0))      AS \"УСХ\"\r\n" +
                "                        FROM\r\n" +
                "                                inv_bt_anal iba\r\n" +
                "                        JOIN\r\n" +
                "                                inv_bt_data ibd\r\n" +
                "                        ON\r\n" +
                "                                iba.anal_id = ibd.anal_id\r\n" +
                "                        WHERE\r\n" +
                "                                iba.btt_id IN ('МПкНаL')\r\n" +
                "                            AND EXISTS\r\n" +
                "                                (\r\n" +
                "                                    SELECT\r\n" +
                "                                            1\r\n" +
                "                                    FROM\r\n" +
                "                                            analog de\r\n" +
                "                                    WHERE\r\n" +
                "                                            de.obj_list_id = iba.obj_1_list_id\r\n" +
                "                                        AND de.obj_id = iba.obj_1_id\r\n" +
                "                                )\r\n" +
               $"                            AND ibd.entry_year = '{argsDT.Rows[0][5]}'\r\n" +
               $"                            AND ibd.entry_month = '{argsDT.Rows[0][4]}'\r\n" +
                "                            AND ibd.entry_day = '0'\r\n" +
                "                        GROUP BY\r\n" +
                "                                iba.obj_1_list_id\r\n" +
                "                            ,   iba.obj_1_id\r\n" +
                "                    ) s\r\n" +
                "                ON\r\n" +
                "                        s.obj_1_list_id = ctr.obj_list_id\r\n" +
                "                    AND s.obj_1_id = ctr.obj_id\r\n" +
                "                ORDER BY\r\n" +
                "                        ctr.spath\r\n" +
                "            ) s\r\n" +
                "    )";
            oracleCommand.ExecuteNonQuery();

            oracleCommand.Connection = oracleConnection;
            oracleCommand.CommandText = "" +

                "SELECT\r\n" +
                "        rn              AS \"№ П/П\"\r\n" +
                "    ,   \"Системный номер\"\r\n" +
                "    ,   \"Компонент\"\r\n" +
                "    ,   \"Количество на 1 единицу\"\r\n" +
                "    ,   RowNo\r\n" +
                "    ,   Parent\r\n" +
                "    ,   PlanOnOrder\r\n" +
                "    ,   QtyNeed\r\n" +
                "    ,   BalanceOnFstDay\r\n" +
                "    ,   QtyComing\r\n" +
                "    ,   QtyOutGo\r\n" +
                "    ,   \"BalanceSRPUSX\"\r\n" +
                "    ,   sNote\r\n" +
                "    ,   NVL(AnotherDivs, 0) AnotherDivs\r\n" +
                "    ,   NVL(s.count_out, 0) count_out\r\n" +
                "FROM\r\n" +
                "        metex.tmp_def21_tree ctr\r\n" +
                "LEFT JOIN\r\n" +
                "    (\r\n" +
                "        SELECT\r\n" +
                "                d.h_anal_6_param1\r\n" +
                "            ,   d.h_anal_6_id        \r\n" +
                "            ,   SUM(di.spec_n_2)    count_out\r\n" +
                "        FROM\r\n" +
                "                doc d\r\n" +
                "        INNER JOIN\r\n" +
                "                doc_item di\r\n" +
                "        ON\r\n" +
                "                di.doc_id = d.doc_id\r\n" +
                "            AND di.tbl_no = 1\r\n" +
                "            AND di.i_anal_3_id = '021'\r\n" +
                "            AND di.spec_d_3 BETWEEN\r\n" +
               $"                    TO_DATE('01.{argsDT.Rows[0][4]}.{argsDT.Rows[0][5]}')\r\n" +
               $"                AND TO_DATE('{argsDT.Rows[0][3]}.{argsDT.Rows[0][4]}.{argsDT.Rows[0][5]}')\r\n" +
                "        WHERE\r\n" +
                "                d.dt_id = 'НМК'\r\n" +
                "            AND d.dtv_id = 'НМК_МЕТ1'\r\n" +
                "            AND d.is_delete = 0\r\n" +
                "            AND d.status != '1101000000'\r\n" +
                "            AND EXISTS\r\n" +
                "                    (\r\n" +
                "                        SELECT\r\n" +
                "                                1\r\n" +
                "                        FROM\r\n" +
                "                            (\r\n" +
                "                                SELECT\r\n" +
               $"                                        REGEXP_SUBSTR('-'||'{argsDT.Rows[0][1]}', '([^-]+)', 1, LEVEL)\r\n" +
                "                                                        AS sOrder\r\n" +
                "                                    ,   1               AS fl\r\n" +
                "                                FROM\r\n" +
                "                                         dual\r\n" +
               $"                                CONNECT BY LEVEL <= REGEXP_COUNT('-'||'{argsDT.Rows[0][1]}', '[^-]+')\r\n" +
                "                            ) ex\r\n" +
                "                        WHERE\r\n" +
                "                                ex.sorder = d.h_anal_4_id\r\n" +
                "                    )\r\n" +
                "            AND EXISTS\r\n" +
                "                    (\r\n" +
                "                        SELECT\r\n" +
                "                                1\r\n" +
                "                        FROM\r\n" +
                "                                metex.tmp_def21_tree ex\r\n" +
                "                        WHERE\r\n" +
                "                                ex.obj_list_id = d.h_anal_6_param1\r\n" +
                "                            AND ex.obj_id = d.h_anal_6_id\r\n" +
                "                    )\r\n" +
                "        GROUP BY\r\n" +
                "                d.h_anal_6_param1\r\n" +
                "            ,   d.h_anal_6_id\r\n" +
                "    ) s\r\n" +
                "ON\r\n" +
                "        s.h_anal_6_param1 = ctr.obj_list_id\r\n" +
                "    AND s.h_anal_6_id = ctr.obj_id\r\n" +
                "ORDER BY\r\n" +
                "        rn"; 
                
            using (OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(command))
            {
                oracleDataAdapter.Fill(dt);
            }
            return dt;
        }

        private void PrdComboBox_SelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            DevExpress.Xpf.Editors.ComboBoxEdit comboBox = sender as DevExpress.Xpf.Editors.ComboBoxEdit;
            DataRow[] selectedRows = ProductDT.Select($"PrdIDName = '{comboBox.SelectedItem?.ToString()}'");
            //MessageBox.Show(comboBox.SelectedItem?.ToString());
            if (selectedRows.Length > 0)
            {
                //MessageBox.Show(reportRows[0][0].ToString()); 
                vm.Source.Clear();
                OrdersViewModel ordersViewModel = new OrdersViewModel();
                OrdComboBox.SelectedItem = null;
                argsDT.Rows[0][0] = selectedRows[0][0];
                ordersDT = GetOrdersDataTable(connection, command, argsDT);
                ordersViewModel.SetOrders(ordersDT);
                OrdComboBox.DataContext = ordersViewModel;
            }
        }
            private void PrdComboBox_EditValueChanging(object sender, DevExpress.Xpf.Editors.EditValueChangingEventArgs e)
        {
            DevExpress.Xpf.Editors.ComboBoxEdit comboBox = sender as DevExpress.Xpf.Editors.ComboBoxEdit;
            DataRow[] filteredRows = ProductDT.Select($"PrdIDName LIKE '%{comboBox.EditValue?.ToString()}%'");
            //MessageBox.Show(comboBox.SelectedItem?.ToString());
            if (filteredRows.Length > 0) 
            {
                System.Data.DataTable filteredTable = filteredRows.CopyToDataTable();
                prdmodel.SetProducts(filteredTable); 
                PrdComboBox.DataContext = prdmodel;
            }
        }

        private void OrdComboBox_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            string strOrders = "", strColName6 = "", strColName12 = "";
            int OrdCount = 0;
            if (dDatePicker.SelectedText != "")
            {
                foreach (var item in OrdComboBox.SelectedItems)
                {
                    strOrders = strOrders + (strOrders != "" ? "-" + item.ToString() : "" + item.ToString());
                    DataRow[] filteredRows = ordersDT.Select($"Order = '{item.ToString()}'");
                    strColName6 = strColName6 + (strColName6 != "" ? "-" + item.ToString() : "" + item.ToString());
                    strColName12 = strColName12 + (strColName12 != "" ? $" + {item.ToString()} {filteredRows[0][1]} шт" : $"{item.ToString()} {filteredRows[0][1]} шт");
                    OrdCount = OrdCount + int.Parse(filteredRows[0][1].ToString());
                }
                tGridControl.Columns[6].Header = $"План на заказ(ы) {strColName6}";
                tGridControl.Columns[13].Header = OrdComboBox.SelectedItems.Count > 1 ? $"{strColName12}\r\n= {OrdCount} шт" : strColName12;
                //MessageBox.Show(strOrders);
                argsDT.Rows[0][1] = strOrders;
                argsDT.Rows[0][2] = OrdCount.ToString();

                GridDT = GetMainDataTable(connection, command, argsDT);
                //MainViewModel vm = new MainViewModel();
                this.DataContext = vm;
                vm.SetItems(GridDT);
            }
            else
            {
                foreach (var item in OrdComboBox.SelectedItems)
                {
                    strOrders = strOrders + (strOrders != "" ? "-" + item.ToString() : "" + item.ToString());
                    DataRow[] filteredRows = ordersDT.Select($"Order = '{item.ToString()}'");
                    strColName6 = strColName6 + (strColName6 != "" ? "+" + item.ToString() : "" + item.ToString());
                    strColName12 = strColName12 + (strColName12 != "" ? $"\r\n+ {item.ToString()} {filteredRows[0][1]} шт" : $"{item.ToString()} {filteredRows[0][1]} шт");
                    OrdCount = OrdCount + int.Parse(filteredRows[0][1].ToString());
                }
                tGridControl.Columns[6].Header = $"План на заказ(ы) {strColName6}";
                tGridControl.Columns[13].Header = OrdComboBox.SelectedItems.Count > 1 ? $"{strColName12}\r\n= {OrdCount} шт" : strColName12;
                //MessageBox.Show(strOrders);
                argsDT.Rows[0][1] = strOrders;
                argsDT.Rows[0][2] = OrdCount.ToString();
                dDatePicker.DateTime = DateTime.Now;
            }    
        }

        private void DateEdit_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            DevExpress.Xpf.Editors.DateEdit datePicker = sender as DevExpress.Xpf.Editors.DateEdit;
            argsDT.Rows[0][3] = datePicker.DateTime.Day.ToString();
            argsDT.Rows[0][4] = datePicker.DateTime.Month.ToString();
            argsDT.Rows[0][5] = datePicker.DateTime.Year.ToString();

            if(OrdComboBox.SelectedItems.Count > 0)
            {
                GridDT = GetMainDataTable(connection, command, argsDT);
                //MainViewModel vm = new MainViewModel();
                this.DataContext = vm;
                vm.SetItems(GridDT);
            }
        }

        private void TreeListView_CellValueChanged(object sender, DevExpress.Xpf.Grid.TreeList.TreeListCellValueChangedEventArgs e)
        {
        }

        private void MemoEdit_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            if (vm.Source.Count > 0)
            {
                sNote = e.NewValue as string;
                DXDivDef.ViewModels.Items currentItem = (DXDivDef.ViewModels.Items)tGridControl.GetRow(tTreeListView.FocusedRowHandle);
                // Получение значения другой ячейки из этой строки
                sPath = currentItem.RowNo;
                //MessageBox.Show("Edited note: " + sNote);
                UpdInsNote(connection, command);
            }
        }
    }
}
