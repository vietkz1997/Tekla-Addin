
template _tmp_0
{
    name = "tpled_template1";
    type = GRAPHICAL;
    width = 60;
    maxheight = 120;
    columns = (1, 1);
    gap = 5;
    fillpolicy = EVEN;
    filldirection = HORIZONTAL;
    margins = (0, 0, 0, 0);
    gridxspacing = 1;
    gridyspacing = 1;
    version = 3.21;
    created = "10.02.2015 14:55";
    modified = "10.02.2015 15:33";
    notes = "";

    row _tmp_1
    {
        name = "PART";
        height = 3.02075439286856;
        visibility = FALSE;
        usecolumns = FALSE;
        rule = "";
        contenttype = "PART";
        sorttype = COMBINE;

        valuefield _tmp_3
        {
            name = "NAME_field";
            location = (1.4954859707272, 0.020754392868559);
            formula = "GetValue(\"NAME\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = RIGHT;
            visibility = TRUE;
            angle = 0;
            length = 3;
            decimals = 2;
            sortdirection = ASCENDING;
            fontname = "fixfont";
            fontcolor = 153;
            fonttype = 4;
            fontsize = 3;
            fontratio = 1.5;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_4
        {
            name = "PART_POS_field";
            location = (15.2453661598258, -2.22044604925031e-016);
            formula = "GetValue(\"PART_POS\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 3;
            decimals = 2;
            sortdirection = ASCENDING;
            fontname = "fixfont";
            fontcolor = 153;
            fonttype = 4;
            fontsize = 3;
            fontratio = 1.5;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        row _tmp_2
        {
            name = "MAIN_BAR";
            height = 3.25814655439146;
            visibility = TRUE;
            usecolumns = FALSE;
            rule = "if (match(GetValue(\"NAME\"),\"MAIN_BAR\"))  then\r\n  Output()\r\nelse\r\n  StepOver()\r\nendif\r\n";
            contenttype = "REBAR";
            sorttype = COMBINE;

            valuefield _tmp_10
            {
                name = "SIZE";
                location = (23.758203935749, 0.128421769364647);
                formula = "GetValue(\"SIZE\")";
                datatype = STRING;
                class = "";
                cacheable = TRUE;
                justify = LEFT;
                visibility = TRUE;
                angle = 0;
                length = 2;
                decimals = 2;
                sortdirection = ASCENDING;
                fontname = "ARIAL";
                fontcolor = 161;
                fonttype = 2;
                fontsize = 3;
                fontratio = 1;
                fontstyle = 0;
                fontslant = 0;
                pen = -1;
                oncombine = NONE;
            };

            valuefield _tmp_8
            {
                name = "NUMBER";
                location = (12.1751543910235, 0.143879037113336);
                formula = "GetValue(\"NUMBER\")";
                datatype = INTEGER;
                class = "";
                cacheable = TRUE;
                justify = RIGHT;
                visibility = TRUE;
                angle = 0;
                length = 2;
                decimals = 2;
                sortdirection = NONE;
                fontname = "ARIAL";
                fontcolor = 161;
                fonttype = 2;
                fontsize = 3;
                fontratio = 1;
                fontstyle = 0;
                fontslant = 0;
                pen = -1;
                oncombine = SUM;
            };

            text _tmp_1
            {
                name = "M.B:";
                x1 = 2.19796204040332;
                y1 = 0.258146554391459;
                x2 = 2.19796204040332;
                y2 = 0.258146554391459;
                string = "M.B:";
                fontname = "ARIAL";
                fontcolor = 161;
                fonttype = 3191148;
                fontsize = 3;
                fontratio = 1;
                fontslant = 0;
                fontstyle = 0;
                angle = 0;
                justify = LEFT;
                pen = -1;
            };

            text _tmp_3
            {
                name = "Text";
                x1 = 16.540115748045;
                y1 = 0.1513897976347;
                x2 = 16.540115748045;
                y2 = 0.1513897976347;
                string = "-DB";
                fontname = "ARIAL";
                fontcolor = 161;
                fonttype = 3191148;
                fontsize = 3;
                fontratio = 1;
                fontslant = 0;
                fontstyle = 0;
                angle = 0;
                justify = LEFT;
                pen = -1;
            };
        };

        row _tmp_0
        {
            name = "HOOP";
            height = 3.44510216216218;
            visibility = TRUE;
            usecolumns = FALSE;
            rule = "if (match(GetValue(\"NAME\"),\"HOOP\"))  then\r\n  Output()\r\nelse\r\n  StepOver()\r\nendif";
            contenttype = "REBAR";
            sorttype = COMBINE;

            text _tmp_9
            {
                name = "Text_1";
                x1 = 2.15689522396759;
                y1 = 0;
                x2 = 2.15689522396759;
                y2 = 0;
                string = "HOOP(ALL):";
                fontname = "ARIAL";
                fontcolor = 161;
                fonttype = 3191148;
                fontsize = 3;
                fontratio = 1;
                fontslant = 0;
                fontstyle = 0;
                angle = 0;
                justify = LEFT;
                pen = -1;
            };

            text _tmp_13
            {
                name = "Text_2";
                x1 = 26.0580481941967;
                y1 = 0;
                x2 = 26.0580481941967;
                y2 = 0;
                string = "DB";
                fontname = "ARIAL";
                fontcolor = 161;
                fonttype = 3191148;
                fontsize = 3;
                fontratio = 1;
                fontslant = 0;
                fontstyle = 0;
                angle = 0;
                justify = LEFT;
                pen = -1;
            };

            valuefield _tmp_17
            {
                name = "ValueField_1";
                location = (31.9270145819042, 0.0345940540540752);
                formula = "GetValue(\"SIZE\")";
                datatype = STRING;
                class = "";
                cacheable = TRUE;
                justify = LEFT;
                visibility = TRUE;
                angle = 0;
                length = 2;
                decimals = 2;
                sortdirection = ASCENDING;
                fontname = "ARIAL";
                fontcolor = 161;
                fonttype = 2;
                fontsize = 3;
                fontratio = 1;
                fontstyle = 0;
                fontslant = 0;
                pen = -1;
                oncombine = NONE;
            };

            text _tmp_19
            {
                name = "Text_3";
                x1 = 37.4132222921525;
                y1 = 0.445102162162183;
                x2 = 37.4132222921525;
                y2 = 0.445102162162183;
                string = "@";
                fontname = "ARIAL";
                fontcolor = 161;
                fonttype = 3191148;
                fontsize = 3;
                fontratio = 1;
                fontslant = 0;
                fontstyle = 0;
                angle = 0;
                justify = LEFT;
                pen = -1;
            };

            valuefield _tmp_21
            {
                name = "ValueField_2";
                location = (42.5592000668816, 0.0766697297297441);
                formula = "GetValue(\"CC_TARGET\")";
                datatype = STRING;
                class = "";
                cacheable = TRUE;
                justify = LEFT;
                visibility = TRUE;
                angle = 0;
                length = 4;
                decimals = 2;
                sortdirection = ASCENDING;
                fontname = "ARIAL";
                fontcolor = 161;
                fonttype = 2;
                fontsize = 3;
                fontratio = 1;
                fontstyle = 0;
                fontslant = 0;
                pen = -1;
                oncombine = NONE;
            };

            valuefield _tmp_23
            {
                name = "ValueField_3";
                location = (51.4370749468377, 0.103048108108123);
                formula = "GetValue(\"REBAR_POS\")";
                datatype = STRING;
                class = "";
                cacheable = TRUE;
                justify = RIGHT;
                visibility = FALSE;
                angle = 0;
                length = 2;
                decimals = 2;
                sortdirection = ASCENDING;
                fontname = "fixfont";
                fontcolor = 153;
                fonttype = 4;
                fontsize = 3;
                fontratio = 1.5;
                fontstyle = 0;
                fontslant = 0;
                pen = -1;
                oncombine = NONE;
            };
        };
    };
};
