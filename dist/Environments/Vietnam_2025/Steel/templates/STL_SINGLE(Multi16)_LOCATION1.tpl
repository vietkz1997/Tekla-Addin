
template _tmp_901
{
    name = "tpled_template1";
    type = GRAPHICAL;
    width = 42.7337753691959;
    maxheight = 150;
    columns = (1, 1);
    gap = 5;
    fillpolicy = EVEN;
    filldirection = HORIZONTAL;
    margins = (0, 0, 0, 0);
    gridxspacing = 0.2;
    gridyspacing = 0.2;
    version = 3.21;
    created = "24.07.2009 16:21";
    modified = "20.05.2011 15:55";
    notes = "";

    row _tmp_1197
    {
        name = "PART_1";
        height = 1;
        visibility = FALSE;
        usecolumns = FALSE;
        rule = "";
        contenttype = "PART";
        sorttype = COMBINE;

        row _tmp_2019
        {
            name = "SIMILAR_PART";
            height = 30.000000970479;
            visibility = TRUE;
            usecolumns = FALSE;
            rule = "";
            contenttype = "SIMILAR_PART";
            sorttype = COMBINE;

            group _tmp_2030
            {
                name = "Group";

                lineorarc _tmp_2031
                {
                    name = "LineOrArc_i0";
                    x1 = 42.7337753691959;
                    y1 = 0;
                    x2 = 42.7337753691959;
                    y2 = 30.000000970479;
                    pen = -1;
                    color = 152;
                    linetype = 1;
                    linewidth = 1;
                    bulge = 0;
                };

                lineorarc _tmp_2032
                {
                    name = "LineOrArc_i1";
                    x1 = 1.12555699161021e-006;
                    y1 = 4.85221335111419e-007;
                    x2 = 1.12555699161021e-006;
                    y2 = 30.000000970479;
                    pen = -1;
                    color = 152;
                    linetype = 1;
                    linewidth = 1;
                    bulge = 0;
                };

                lineorarc _tmp_2033
                {
                    name = "LineOrArc_i2";
                    x1 = 1.12555699161021e-006;
                    y1 = 5.00000097044995;
                    x2 = 42.7337753691959;
                    y2 = 5.00000097044995;
                    pen = -1;
                    color = 152;
                    linetype = 1;
                    linewidth = 1;
                    bulge = 0;
                };

                lineorarc _tmp_2034
                {
                    name = "LineOrArc_i3";
                    x1 = 1.12555699161021e-006;
                    y1 = 10.0000009704499;
                    x2 = 42.7337753691959;
                    y2 = 10.0000009704499;
                    pen = -1;
                    color = 152;
                    linetype = 1;
                    linewidth = 1;
                    bulge = 0;
                };

                lineorarc _tmp_2035
                {
                    name = "LineOrArc_i4";
                    x1 = 0;
                    y1 = 15.0000009704609;
                    x2 = 42.7337753691959;
                    y2 = 15.0000009704611;
                    pen = -1;
                    color = 152;
                    linetype = 1;
                    linewidth = 1;
                    bulge = 0;
                };

                lineorarc _tmp_2036
                {
                    name = "LineOrArc_i5";
                    x1 = 1.12555699161021e-006;
                    y1 = 20.0000009704681;
                    x2 = 42.7337753691959;
                    y2 = 20.0000009704672;
                    pen = -1;
                    color = 152;
                    linetype = 1;
                    linewidth = 1;
                    bulge = 0;
                };

                lineorarc _tmp_2037
                {
                    name = "LineOrArc_i6";
                    x1 = 1.12555699161021e-006;
                    y1 = 25.0000009704754;
                    x2 = 42.7337753691959;
                    y2 = 25.0000009704754;
                    pen = -1;
                    color = 152;
                    linetype = 1;
                    linewidth = 1;
                    bulge = 0;
                };

                lineorarc _tmp_2038
                {
                    name = "LineOrArc_i7";
                    x1 = 1.12555699161021e-006;
                    y1 = 30.000000970479;
                    x2 = 42.7337753691959;
                    y2 = 30.000000970479;
                    pen = -1;
                    color = 152;
                    linetype = 1;
                    linewidth = 1;
                    bulge = 0;
                };

                lineorarc _tmp_2039
                {
                    name = "LineOrArc_i8";
                    x1 = 42.7337753691959;
                    y1 = 0;
                    x2 = 1.12555699161021e-006;
                    y2 = 0;
                    pen = -1;
                    color = 152;
                    linetype = 1;
                    linewidth = 1;
                    bulge = 0;
                };
            };

            valuefield _tmp_2046
            {
                name = "ValueField_2";
                location = (16.6, 1);
                formula = "GetValue(\"MODEL_TOTAL\")";
                datatype = INTEGER;
                class = "";
                cacheable = TRUE;
                justify = CENTERED;
                visibility = TRUE;
                angle = 0;
                length = 4;
                decimals = 0;
                sortdirection = NONE;
                fontname = "romsim";
                fontcolor = 161;
                fonttype = 4;
                fontsize = 3;
                fontratio = 0.8;
                fontstyle = 0;
                fontslant = 0;
                pen = -1;
                oncombine = NONE;
            };

            valuefield _tmp_2048
            {
                name = "ValueField_3";
                location = (11.6, 11);
                formula = "GetValue(\"NAME\")";
                datatype = STRING;
                class = "";
                cacheable = TRUE;
                justify = CENTERED;
                visibility = TRUE;
                angle = 0;
                length = 9;
                decimals = 0;
                sortdirection = NONE;
                fontname = "romsim";
                fontcolor = 161;
                fonttype = 4;
                fontsize = 3;
                fontratio = 0.8;
                fontstyle = 0;
                fontslant = 0;
                pen = -1;
                oncombine = NONE;
            };

            valuefield _tmp_2050
            {
                name = "ValueField_4";
                location = (14.1, 21);
                formula = "GetValue(\"PART_POS\")";
                datatype = STRING;
                class = "";
                cacheable = TRUE;
                justify = CENTERED;
                visibility = TRUE;
                angle = 0;
                length = 7;
                decimals = 0;
                sortdirection = NONE;
                fontname = "romsim";
                fontcolor = 161;
                fonttype = 4;
                fontsize = 3;
                fontratio = 0.8;
                fontstyle = 0;
                fontslant = 0;
                pen = -1;
                oncombine = NONE;
            };
        };
    };
};
