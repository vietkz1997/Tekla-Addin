
template _tmp_883
{
    name = "tpled_template1";
    type = GRAPHICAL;
    width = 65.0184950513656;
    maxheight = 120;
    columns = (1, 1);
    gap = 5;
    fillpolicy = EVEN;
    filldirection = HORIZONTAL;
    margins = (0, 0, 0, 0);
    gridxspacing = 5;
    gridyspacing = 5;
    version = 3.21;
    created = "25.11.2009 20:13";
    modified = "13.06.2013 18:16";
    notes = "";

    row _tmp_913
    {
        name = "CAST_UNIT";
        height = 14.97;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "";
        contenttype = "CAST_UNIT";
        sorttype = COMBINE;

        lineorarc _tmp_914
        {
            name = "LineOrArc_i0";
            x1 = 65.0184950513656;
            y1 = 14.97;
            x2 = 0;
            y2 = 14.97;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_915
        {
            name = "LineOrArc_i1";
            x1 = 65.0184950513656;
            y1 = 6.98599999999999;
            x2 = 0;
            y2 = 6.98599999999999;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_916
        {
            name = "LineOrArc_i2";
            x1 = 65.0184950513656;
            y1 = 0;
            x2 = 0;
            y2 = 0;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        text _tmp_917
        {
            name = "CONCRETE QUANTITY";
            x1 = 14.4930970514106;
            y1 = 9.73599685637004;
            x2 = 14.4930970514106;
            y2 = 9.73599685637004;
            string = "CONCRETE QUANTITY";
            fontname = "romsim";
            fontcolor = 153;
            fonttype = 4;
            fontsize = 2.5;
            fontratio = 0.85;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        lineorarc _tmp_918
        {
            name = "LineOrArc_i4";
            x1 = 0;
            y1 = 0;
            x2 = 0;
            y2 = 14.97;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_921
        {
            name = "LineOrArc_i7";
            x1 = 32.8828950513687;
            y1 = 6.98599999999999;
            x2 = 32.8828950513689;
            y2 = 5.6843418860808e-014;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        text _tmp_922
        {
            name = "3";
            x1 = 61.1500694208105;
            y1 = 3.74021282184453;
            x2 = 61.1500694208105;
            y2 = 3.74021282184453;
            string = "3";
            fontname = "romsim";
            fontcolor = 161;
            fonttype = 4;
            fontsize = 1.5;
            fontratio = 1;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_923
        {
            name = "M";
            x1 = 59.1593329456832;
            y1 = 2.12239728996585;
            x2 = 59.1593329456832;
            y2 = 2.12239728996585;
            string = "M";
            fontname = "romsim";
            fontcolor = 161;
            fonttype = 4;
            fontsize = 2;
            fontratio = 0.85;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        lineorarc _tmp_924
        {
            name = "LineOrArc_i10";
            x1 = 65.0184950513656;
            y1 = 14.97;
            x2 = 65.0184950513656;
            y2 = 0;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        text _tmp_895
        {
            name = "Text";
            x1 = 6.29011645825994;
            y1 = 2.41360294117647;
            x2 = 6.29011645825994;
            y2 = 2.41360294117647;
            string = "fck=      Mpa";
            fontname = "romsim";
            fontcolor = 161;
            fonttype = 4;
            fontsize = 2;
            fontratio = 0.85;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        valuefield _tmp_896
        {
            name = "MATERIAL_field";
            location = (11.8244980626981, 2.27757352941176);
            formula = "mid(GetValue(\"MATERIAL\"), 1 , 2)";
            datatype = INTEGER;
            class = "";
            cacheable = TRUE;
            justify = CENTERED;
            visibility = TRUE;
            angle = 0;
            length = 5;
            decimals = 2;
            sortdirection = NONE;
            fontname = "romsim";
            fontcolor = 161;
            fonttype = 4;
            fontsize = 2;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_893
        {
            name = "ValueField";
            location = (42.4607989902547, 2.18235294117647);
            formula = "GetValue(\"VOLUME\")";
            datatype = DOUBLE;
            class = "Volume";
            cacheable = TRUE;
            justify = CENTERED;
            visibility = TRUE;
            angle = 0;
            length = 8;
            decimals = 2;
            sortdirection = NONE;
            fontname = "romsim";
            fontcolor = 161;
            fonttype = 4;
            fontsize = 2;
            fontratio = 1;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
            unit = "m3";
        };
    };
};
