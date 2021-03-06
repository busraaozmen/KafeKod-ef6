﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KafeKod.Data;
using KafeKod.Properties;
using Newtonsoft.Json;

namespace KafeKod
{
    public partial class Form1 : Form
    {
        private KafeVeri db;

        public Form1()
        {
            //db = new KafeVeri();
            //OrnekVerileriYukle();
            VerileriOku();
            InitializeComponent();
            MasalariOlustur();
        }

        private void VerileriOku()
        {
            try
            {
                string json = File.ReadAllText("veri.json");
                db = JsonConvert.DeserializeObject<KafeVeri>(json);
            }
            catch (Exception)
            {
                db = new KafeVeri();
            }
        }

        private void OrnekVerileriYukle()
        {
            db.Urunler = new List<Urun>()
            {
                new Urun(){UrunAd = "Kola", BirimFiyat = 6.99m},
                new Urun(){UrunAd = "Çay", BirimFiyat = 9.99m}
            };
            db.Urunler.Sort();
        }

        private void MasalariOlustur()
        {

            #region ListView Imajlarının Hazırlanması
            ImageList il = new ImageList();
            il.Images.Add("bos", Properties.Resources.masabos);
            il.Images.Add("dolu", Properties.Resources.masadolu);
            il.ImageSize = new Size(64, 64);
            lvwMasalar.LargeImageList = il;
            #endregion



            ListViewItem lvi;

            for (int masaNo = 1; masaNo <= db.MasaAdet; masaNo++)
            {
                lvi = new ListViewItem("Masa " + masaNo);
                //masaNo değeriyle kayıtlı bir sipariş var mı ?
                Siparis sip = db.AktifSiparisler.FirstOrDefault(x => x.MasaNo == masaNo);

                if (sip == null)
                {
                    lvi.Tag = masaNo;
                    lvi.ImageKey = "bos";
                }

                else
                {
                    lvi.Tag = sip;
                    lvi.ImageKey = "dolu";
                }
                lvwMasalar.Items.Add(lvi);
            }
        }

        private void lvwMasalar_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var lvi = lvwMasalar.SelectedItems[0];
                    lvwMasalar.SelectedItems[0].ImageKey = "dolu";

                Siparis sip;
                // masada doluysa olanı al, boşsa yeni oluştur
                if (lvi.Tag is Siparis)
                {
                    sip = (Siparis)lvi.Tag;
                }
                else
                {
                    sip = new Siparis();
                    sip.MasaNo = (int) lvi.Tag;
                    sip.AcilisZamani = DateTime.Now;
                    lvi.Tag = sip;
                    db.AktifSiparisler.Add(sip);
                }

                // sipariş formun oluşma ani
                SiparisForm frmSiparis = new SiparisForm(db,sip);
                frmSiparis.MasaTasiniyor += FrmSiparis_MasaTasindi;
                frmSiparis.ShowDialog();


                if (sip.Durum != SiparisDurum.Aktif)
                {
                    lvi.Tag = sip.MasaNo;
                    lvi.ImageKey = "bos";
                    db.AktifSiparisler.Remove(sip);
                    db.GecmisSiparisler.Add(sip);
                }
            }
        }

        private void FrmSiparis_MasaTasindi(object sender, MasaTasimaEventArgs e)
        {
            // adım 1: eski masayı boşalt

            ListViewItem lviEskiMasa = MasaBul(e.EskiMasaNo);

            lviEskiMasa.Tag = e.EskiMasaNo;
            lviEskiMasa.ImageKey = "bos";

            // adım 2: yeni masaya siparişi koy

            ListViewItem lviYeniMasa = MasaBul(e.YeniMasaNo);

            lviYeniMasa.Tag = e.TasinanSiparis;
            lviYeniMasa.ImageKey = "dolu";
        }


        private void tsmiGecmisSiparisler_Click(object sender, EventArgs e)
        {
            var frm = new GecmisSiparislerForm(db);
            frm.ShowDialog();
        }

        private void tsmiUrunler_Click(object sender, EventArgs e)
        {
            var frm = new UrunlerForm(db);
            frm.ShowDialog();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            string json = JsonConvert.SerializeObject(db);
            File.WriteAllText("veri.json",json);
        }

        private ListViewItem MasaBul(int masaNo)
        {
            foreach (ListViewItem item in lvwMasalar.Items)
            {
                if (item.Tag is int && (int)item.Tag == masaNo)
                {
                    return item;
                }
                else if (item.Tag is Siparis && ((Siparis)item.Tag).MasaNo == masaNo)
                {
                    return item;
                }
            }
            return null;
        }
    }
}

