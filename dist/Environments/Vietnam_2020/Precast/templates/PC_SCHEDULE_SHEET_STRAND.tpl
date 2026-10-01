
template _tmp_1030
{
    name = "tpled_template2";
    type = GRAPHICAL;
    width = 197.655;
    maxheight = 200;
    columns = (1, 1);
    gap = 5;
    fillpolicy = EVEN;
    filldirection = HORIZONTAL;
    margins = (0, 0, 0, 0);
    gridxspacing = 5;
    gridyspacing = 5;
    version = 3.21;
    created = "25.11.2009 22:24";
    modified = "01.04.2013 16:00";
    notes = "";

    row _tmp_4740
    {
        name = "STRAND_1";
        height = 17.964;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "if (GetValue(\"SIZE\") == 15.2) then\r\n  Output()\r\nelse\r\n  StepOver()\r\nendif\r\n";
        contenttype = "STRAND";
        sorttype = COMBINE;

        lineorarc _tmp_4771
        {
            name = "LineOrArc_11";
            x1 = 4.54747350886464e-013;
            y1 = 0;
            x2 = 197.654441118711;
            y2 = 0;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_4772
        {
            name = "LineOrArc_12";
            x1 = 4.54747350886464e-013;
            y1 = 17.9640000000001;
            x2 = 197.654441118711;
            y2 = 17.9640000000001;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_4773
        {
            name = "LineOrArc_13";
            x1 = 16.8168778432546;
            y1 = 0;
            x2 = 16.8168778432548;
            y2 = 17.9640000000001;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_4774
        {
            name = "LineOrArc_14";
            x1 = 42.4277671612617;
            y1 = 0;
            x2 = 42.4277671612615;
            y2 = 17.9640000000001;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_4775
        {
            name = "LineOrArc_15";
            x1 = 29.0053039205241;
            y1 = 0;
            x2 = 29.0053039205236;
            y2 = 17.9640000000001;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_4776
        {
            name = "LineOrArc_16";
            x1 = 58.0858646722629;
            y1 = 0;
            x2 = 58.0858646722629;
            y2 = 17.9640000000001;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_4777
        {
            name = "LineOrArc_17";
            x1 = 0;
            y1 = 0;
            x2 = 2.27373675443232e-013;
            y2 = 17.9640000000002;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        text _tmp_4778
        {
            name = "Text_19";
            x1 = 5.35556438335881;
            y1 = 7.96772232399922;
            x2 = 5.35556438335881;
            y2 = 7.96772232399922;
            string = "MARK";
            fontname = "Arial";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 0.85;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_4779
        {
            name = "Text_20";
            x1 = 20.4182630656293;
            y1 = 7.96772232399922;
            x2 = 20.4182630656293;
            y2 = 7.96772232399922;
            string = "DIA.";
            fontname = "Arial";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 0.85;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_4780
        {
            name = "Text_21";
            x1 = 45.4169445404387;
            y1 = 10.0552899450451;
            x2 = 45.4169445404387;
            y2 = 10.0552899450451;
            string = "SHAPE";
            fontname = "Arial";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 0.85;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_4781
        {
            name = "Text_22";
            x1 = 31.5649322323485;
            y1 = 9.80753300154259;
            x2 = 31.5649322323485;
            y2 = 9.80753300154259;
            string = "REQ'D";
            fontname = "Arial";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 0.85;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_4782
        {
            name = "Text_23";
            x1 = 46.2628683499653;
            y1 = 5.35214749365;
            x2 = 46.2628683499653;
            y2 = 5.35214749365;
            string = "CODE";
            fontname = "Arial";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 0.85;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_4783
        {
            name = "Text_24";
            x1 = 33.7664225816023;
            y1 = 5.54240955504472;
            x2 = 33.7664225816023;
            y2 = 5.54240955504472;
            string = "NO.";
            fontname = "Arial";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 0.85;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        lineorarc _tmp_4784
        {
            name = "LineOrArc_18";
            x1 = 95.4360181847139;
            y1 = 0;
            x2 = 95.4360181847139;
            y2 = 17.9640000000001;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_4785
        {
            name = "LineOrArc_19";
            x1 = 76.6360693152085;
            y1 = 5.6843418860808e-014;
            x2 = 76.6360693152085;
            y2 = 17.9640000000002;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        text _tmp_4786
        {
            name = "Text_25";
            x1 = 163.429300610894;
            y1 = 8.00327755380539;
            x2 = 163.429300610894;
            y2 = 8.00327755380539;
            string = "BendingShape";
            fontname = "Arial";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 0.85;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_4787
        {
            name = "Text_26";
            x1 = 63.8687681305034;
            y1 = 12.1379418256092;
            x2 = 63.8687681305034;
            y2 = 12.1379418256092;
            string = "CUT";
            fontname = "Arial";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 0.85;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_4788
        {
            name = "Text_27";
            x1 = 64.7817422215924;
            y1 = 3.62649846482671;
            x2 = 64.7817422215924;
            y2 = 3.62649846482671;
            string = "(M)";
            fontname = "Arial";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 0.85;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_4789
        {
            name = "Text_28";
            x1 = 63.186388790951;
            y1 = 7.54386733557061;
            x2 = 63.186388790951;
            y2 = 7.54386733557061;
            string = "LENG.";
            fontname = "Arial";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 0.85;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        lineorarc _tmp_4790
        {
            name = "LineOrArc_20";
            x1 = 121.113332857445;
            y1 = 0;
            x2 = 121.113332857445;
            y2 = 17.9640000000001;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        text _tmp_4791
        {
            name = "Text_29";
            x1 = 104.48278844698;
            y1 = 3.52820836594102;
            x2 = 104.48278844698;
            y2 = 3.52820836594102;
            string = "(Kg)";
            fontname = "Arial";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 0.85;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_4792
        {
            name = "Text_30";
            x1 = 104.301681022551;
            y1 = 12.1379418256092;
            x2 = 104.301681022551;
            y2 = 12.1379418256092;
            string = "Unit";
            fontname = "Arial";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 0.85;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_4793
        {
            name = "Text_31";
            x1 = 103.362933539883;
            y1 = 7.95962887190029;
            x2 = 103.362933539883;
            y2 = 7.95962887190029;
            string = "Weight";
            fontname = "Arial";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 0.85;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_4794
        {
            name = "Text_32";
            x1 = 84.1620117642362;
            y1 = 3.52820836594102;
            x2 = 84.1620117642362;
            y2 = 3.52820836594102;
            string = "(M)";
            fontname = "Arial";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 0.85;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_4795
        {
            name = "Text_33";
            x1 = 81.8601543398074;
            y1 = 12.1379418256092;
            x2 = 81.8601543398074;
            y2 = 12.1379418256092;
            string = "TOTAL";
            fontname = "Arial";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 0.85;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_4796
        {
            name = "Text_34";
            x1 = 82.7896866190447;
            y1 = 7.54379553856705;
            x2 = 82.7896866190447;
            y2 = 7.54379553856705;
            string = "LENG.";
            fontname = "Arial";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 0.85;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        lineorarc _tmp_4797
        {
            name = "LineOrArc_21";
            x1 = 146.790647530177;
            y1 = 5.6843418860808e-014;
            x2 = 146.790647530177;
            y2 = 17.9640000000002;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        text _tmp_4798
        {
            name = "Text_35";
            x1 = 130.160103119711;
            y1 = 3.52820836594108;
            x2 = 130.160103119711;
            y2 = 3.52820836594108;
            string = "(Kg)";
            fontname = "Arial";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 0.85;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_4799
        {
            name = "Text_36";
            x1 = 129.57504331433;
            y1 = 12.1379418256093;
            x2 = 129.57504331433;
            y2 = 12.1379418256093;
            string = "Total";
            fontname = "Arial";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 0.85;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_4800
        {
            name = "Text_37";
            x1 = 129.040248212615;
            y1 = 7.95962887190041;
            x2 = 129.040248212615;
            y2 = 7.95962887190041;
            string = "Weight";
            fontname = "Arial";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 0.85;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };
    };

    row _tmp_959
    {
        name = "STRAND";
        height = 18.9553129926753;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "";
        contenttype = "STRAND";
        sorttype = COMBINE;

        valuefield _tmp_960
        {
            name = "ValueField";
            location = (18.455171798561, 7.69090064446221);
            formula = "GetValue(\"SIZE\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 5;
            decimals = 0;
            sortdirection = ASCENDING;
            fontname = "Arial";
            fontcolor = 161;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = 6;
            oncombine = NONE;
        };

        valuefield _tmp_961
        {
            name = "ValueField_1";
            location = (1.57747395683442, 7.69090064446221);
            formula = "GetValue(\"REBAR_POS\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = CENTERED;
            visibility = TRUE;
            angle = 0;
            length = 8;
            sortdirection = ASCENDING;
            fontname = "Arial";
            fontcolor = 161;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = 6;
            oncombine = NONE;
        };

        valuefield _tmp_966
        {
            name = "ValueField_2";
            location = (31.4632293525179, 7.69090064446221);
            formula = "GetValue(\"NUMBER\")";
            datatype = INTEGER;
            class = "";
            cacheable = TRUE;
            justify = CENTERED;
            visibility = TRUE;
            angle = 0;
            length = 5;
            decimals = 0;
            sortdirection = NONE;
            fontname = "Arial";
            fontcolor = 161;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = 6;
            oncombine = SUM;
        };

        valuefield _tmp_967
        {
            name = "ValueField_3";
            location = (46.3721563701694, 7.69090064446221);
            formula = "GetValue(\"SHAPE\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = CENTERED;
            visibility = TRUE;
            angle = 0;
            length = 4;
            sortdirection = NONE;
            fontname = "Arial";
            fontcolor = 161;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = 6;
            oncombine = NONE;
        };

        valuefield _tmp_968
        {
            name = "ValueField_4";
            location = (59.8764777175262, 7.69090064446221);
            formula = "GetValue(\"LENGTH\")";
            datatype = DOUBLE;
            class = "Length";
            cacheable = TRUE;
            justify = RIGHT;
            visibility = TRUE;
            angle = 0;
            length = 8;
            decimals = 3;
            sortdirection = NONE;
            fontname = "Arial";
            fontcolor = 161;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = 6;
            oncombine = NONE;
            unit = "m";
        };

        valuefield _tmp_969
        {
            name = "ValueField_5";
            location = (79.5846106926535, 7.69090064446221);
            formula = "GetValue(\"LENGTH\")*GetValue(\"NUMBER\")";
            datatype = DOUBLE;
            class = "Length";
            cacheable = TRUE;
            justify = RIGHT;
            visibility = TRUE;
            angle = 0;
            length = 8;
            decimals = 3;
            sortdirection = NONE;
            fontname = "Arial";
            fontcolor = 161;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = 6;
            oncombine = NONE;
            unit = "m";
        };

        valuefield _tmp_970
        {
            name = "ValueField_6";
            location = (101.42993393946, 7.69090064446221);
            formula = "GetValue(\"WEIGHT\")";
            datatype = DOUBLE;
            class = "Weight";
            cacheable = TRUE;
            justify = RIGHT;
            visibility = TRUE;
            angle = 0;
            length = 8;
            decimals = 3;
            sortdirection = NONE;
            fontname = "Arial";
            fontcolor = 161;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = 6;
            oncombine = NONE;
            unit = "kg";
        };

        group _tmp_971
        {
            name = "Group_1";

            lineorarc _tmp_972
            {
                name = "LineOrArc";
                x1 = 4.54747350886464e-013;
                y1 = 0;
                x2 = 197.654441118711;
                y2 = 0;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_973
            {
                name = "LineOrArc";
                x1 = 4.54747350886464e-013;
                y1 = 18.9553129926751;
                x2 = 197.654441118711;
                y2 = 18.9553129926751;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_974
            {
                name = "LineOrArc";
                x1 = 16.8168778432546;
                y1 = 0;
                x2 = 16.8168778432548;
                y2 = 18.9553129926751;
                pen = -1;
                color = 164;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_975
            {
                name = "LineOrArc";
                x1 = 42.4277671612617;
                y1 = 0;
                x2 = 42.4277671612615;
                y2 = 18.9553129926751;
                pen = -1;
                color = 164;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_976
            {
                name = "LineOrArc";
                x1 = 29.0053039205241;
                y1 = 0;
                x2 = 29.0053039205236;
                y2 = 18.9553129926751;
                pen = -1;
                color = 164;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_977
            {
                name = "LineOrArc";
                x1 = 58.0858646722629;
                y1 = 0;
                x2 = 58.0858646722629;
                y2 = 18.9553129926751;
                pen = -1;
                color = 164;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_978
            {
                name = "LineOrArc";
                x1 = 0;
                y1 = 0;
                x2 = 2.27373675443232e-013;
                y2 = 18.9553129926753;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_979
            {
                name = "LineOrArc_i7";
                x1 = 95.4360181847139;
                y1 = 0;
                x2 = 95.4360181847139;
                y2 = 18.9553129926751;
                pen = -1;
                color = 164;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_980
            {
                name = "LineOrArc_i8";
                x1 = 76.6360693152085;
                y1 = 5.99802269027134e-014;
                x2 = 76.6360693152085;
                y2 = 18.9553129926753;
                pen = -1;
                color = 164;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_981
            {
                name = "LineOrArc_i9";
                x1 = 121.113332857445;
                y1 = 0;
                x2 = 121.113332857445;
                y2 = 18.9553129926751;
                pen = -1;
                color = 164;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_982
            {
                name = "LineOrArc_i10";
                x1 = 146.790647530177;
                y1 = 5.99802269027134e-014;
                x2 = 146.790647530177;
                y2 = 18.9553129926753;
                pen = -1;
                color = 164;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };
        };

        valuefield _tmp_983
        {
            name = "ValueField_7";
            location = (126.109671616492, 7.69090064446221);
            formula = "GetFieldFormula(\"NUMBER\")*GetFieldFormula(\"WEIGHT_MAX_field\")";
            datatype = DOUBLE;
            class = "Weight";
            cacheable = TRUE;
            justify = RIGHT;
            visibility = TRUE;
            angle = 0;
            length = 8;
            decimals = 3;
            sortdirection = NONE;
            fontname = "Arial";
            fontcolor = 161;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = 6;
            oncombine = SUM;
            unit = "kg";
        };
    };
};
