
template _tmp_1
{
    name = "tpled_template1";
    type = GRAPHICAL;
    width = 58.6646048509771;
    maxheight = 32;
    columns = (1, 1);
    gap = 5;
    fillpolicy = EVEN;
    filldirection = HORIZONTAL;
    margins = (0, 0, 0, 0);
    gridxspacing = 2;
    gridyspacing = 2;
    version = 3.21;
    created = "17.09.2015 13:44";
    modified = "17.09.2015 14:06";
    notes = "";

    row _tmp_2
    {
        name = "CAST_UNIT";
        height = 4;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "";
        contenttype = "PART";
        sorttype = COMBINE;

        row _tmp_20
        {
            name = "MAIN_BAR";
            height = 7;
            visibility = TRUE;
            usecolumns = FALSE;
            rule = "";
            contenttype = "REBAR";
            sorttype = COMBINE;

            valuefield _tmp_21
            {
                name = "NUMBER";
                location = (10.0452672024153, 3.02866565871737);
                formula = "GetValue(\"NUMBER\")";
                datatype = INTEGER;
                class = "";
                cacheable = TRUE;
                justify = LEFT;
                visibility = TRUE;
                angle = 0;
                length = 3;
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

            valuefield _tmp_22
            {
                name = "ValueField_1";
                location = (2.40411541293546, 3.25747893672566);
                formula = "GetValue(\"REBAR_POS\")";
                datatype = STRING;
                class = "";
                cacheable = TRUE;
                justify = CENTERED;
                visibility = TRUE;
                angle = 0;
                length = 3;
                decimals = 2;
                sortdirection = ASCENDING;
                fontname = "ARIAL";
                fontcolor = 161;
                fonttype = 0;
                fontsize = 2.5;
                fontratio = 1;
                fontstyle = 0;
                fontslant = 0;
                pen = -1;
                oncombine = NONE;
            };

            circle _tmp_23
            {
                name = "Circle";
                radius = 2.6;
                center = (4.91851756335656, 4.37929908764125);
                filled = FALSE;
                filltype = -1;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
            };

            valuefield _tmp_24
            {
                name = "SIZE";
                location = (17.4544966941092, 2.9785734488939);
                formula = "\"-HD\" + GetValue(\"SIZE\")";
                datatype = STRING;
                class = "";
                cacheable = TRUE;
                justify = LEFT;
                visibility = TRUE;
                angle = 0;
                length = 5;
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
                oncombine = NONE;
            };

            valuefield _tmp_25
            {
                name = "ValueField";
                location = (29.7769110615337, 2.86858086375801);
                formula = "GetValue(\"LENGTH\")";
                datatype = INTEGER;
                class = "Length";
                cacheable = TRUE;
                justify = RIGHT;
                visibility = TRUE;
                angle = 0;
                length = 6;
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
                oncombine = NONE;
                unit = "mm";
            };

            text _tmp_26
            {
                name = "Text_2";
                x1 = 43.1766207093309;
                y1 = 3.65412187875962;
                x2 = 43.1766207093309;
                y2 = 3.65412187875962;
                string = "@";
                fontname = "ARIAL";
                fontcolor = 161;
                fonttype = 0;
                fontsize = 2;
                fontratio = 1;
                fontslant = 0;
                fontstyle = 0;
                angle = 0;
                justify = LEFT;
                pen = -1;
            };

            valuefield _tmp_27
            {
                name = "ValueField_2";
                location = (46.6587454759771, 2.97812443222274);
                formula = "GetValue(\"CC_TARGET\")";
                datatype = STRING;
                class = "";
                cacheable = TRUE;
                justify = LEFT;
                visibility = TRUE;
                angle = 0;
                length = 6;
                decimals = 0;
                sortdirection = NONE;
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

            valuefield _tmp_28
            {
                name = "ValueField_3";
                location = (11.2811225064107, 0.484201446635072);
                formula = "GetValue(\"SHAPE\")";
                datatype = STRING;
                class = "";
                cacheable = TRUE;
                justify = RIGHT;
                visibility = TRUE;
                angle = 0;
                length = 3;
                decimals = 2;
                sortdirection = NONE;
                fontname = "ARIAL";
                fontcolor = 161;
                fonttype = 0;
                fontsize = 2;
                fontratio = 1;
                fontstyle = 0;
                fontslant = 0;
                pen = -1;
                oncombine = NONE;
            };

            valuefield _tmp_29
            {
                name = "ValueField_4";
                location = (17.6208764558551, 0.463071963465833);
                formula = "GetValue(\"DIM_B\")";
                datatype = STRING;
                class = "";
                cacheable = TRUE;
                justify = CENTERED;
                visibility = TRUE;
                angle = 0;
                length = 3;
                decimals = 2;
                sortdirection = NONE;
                fontname = "ARIAL";
                fontcolor = 161;
                fonttype = 0;
                fontsize = 2;
                fontratio = 1;
                fontstyle = 0;
                fontslant = 0;
                pen = -1;
                oncombine = NONE;
            };

            valuefield _tmp_30
            {
                name = "ValueField_5";
                location = (22.7453003257753, 0.414123155491165);
                formula = "GetValue(\"DIM_C\")";
                datatype = STRING;
                class = "";
                cacheable = TRUE;
                justify = CENTERED;
                visibility = TRUE;
                angle = 0;
                length = 5;
                decimals = 2;
                sortdirection = NONE;
                fontname = "ARIAL";
                fontcolor = 161;
                fonttype = 0;
                fontsize = 2;
                fontratio = 1;
                fontstyle = 0;
                fontslant = 0;
                pen = -1;
                oncombine = NONE;
            };

            valuefield _tmp_31
            {
                name = "ValueField_6";
                location = (30.5661810710703, 0.429546889931002);
                formula = "GetValue(\"DIM_D\")";
                datatype = STRING;
                class = "";
                cacheable = TRUE;
                justify = CENTERED;
                visibility = TRUE;
                angle = 0;
                length = 3;
                decimals = 2;
                sortdirection = NONE;
                fontname = "ARIAL";
                fontcolor = 161;
                fonttype = 0;
                fontsize = 2;
                fontratio = 1;
                fontstyle = 0;
                fontslant = 0;
                pen = -1;
                oncombine = NONE;
            };

            text _tmp_32
            {
                name = "Text";
                x1 = 8.78292738946383;
                y1 = 3.73476331950208;
                x2 = 8.78292738946383;
                y2 = 3.73476331950208;
                string = "-";
                fontname = "ARIAL";
                fontcolor = 161;
                fonttype = 0;
                fontsize = 2;
                fontratio = 1;
                fontslant = 0;
                fontstyle = 0;
                angle = 0;
                justify = LEFT;
                pen = -1;
            };
        };

        valuefield _tmp_43
        {
            name = "ValueField_9";
            location = (24.112140597764, 0.660667877788988);
            formula = "GetValue(\"SIMILAR_TO_MAIN_PART\")";
            datatype = INTEGER;
            class = "";
            cacheable = TRUE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 4;
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

        valuefield _tmp_45
        {
            name = "ValueField_10";
            location = (34.4969224285774, 0.749665493788996);
            formula = "GetValue(\"NUMBER\")";
            datatype = INTEGER;
            class = "";
            cacheable = TRUE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 4;
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
    };
};
