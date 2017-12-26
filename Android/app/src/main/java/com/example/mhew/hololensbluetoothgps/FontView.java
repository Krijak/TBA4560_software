package com.example.mhew.hololensbluetoothgps;

import android.content.Context;
import android.graphics.Typeface;
import android.util.AttributeSet;
import android.widget.TextView;

/**
 * Created by krist on 14.12.2017.
 */

public class FontView extends android.support.v7.widget.AppCompatTextView {

    public FontView(Context context) {
        super(context);

        applyCustomFont(context);
    }

    public FontView(Context context, AttributeSet attrs) {
        super(context, attrs);

        applyCustomFont(context);
    }

    public FontView(Context context, AttributeSet attrs, int defStyle) {
        super(context, attrs, defStyle);

        applyCustomFont(context);
    }

    private void applyCustomFont(Context context) {
        Typeface face = Typeface.createFromAsset(context.getAssets(), "robotothin.ttf");
        setTypeface(face);
    }
}